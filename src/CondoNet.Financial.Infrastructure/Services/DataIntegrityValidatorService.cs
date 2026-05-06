using CondoNet.BlockChain.Core;
using CondoNet.Financial.Core.Entities;
using CondoNet.Financial.Core.Interfaces;
using CondoNet.Financial.Core.Services;

namespace CondoNet.Financial.Infrastructure.Services;

public class DataIntegrityValidatorService(
    IInvoiceRepository invoiceRepo,
    IBlockchainIntegrationService blockchainIntegrationService,
    IAuditLogRepository auditLogRepository,
    IUnitAccountRepository unitAccountRepo,
    ITransactionRepository transactionRepo) : IDataIntegrityValidatorService
{
    private readonly IInvoiceRepository _invoiceRepo = invoiceRepo;
    private readonly IBlockchainIntegrationService _blockchainIntegrationService = blockchainIntegrationService;
    private readonly IAuditLogRepository _auditLogRepository = auditLogRepository;
    private readonly IUnitAccountRepository _unitAccountRepo = unitAccountRepo;
    private readonly ITransactionRepository _transactionRepo = transactionRepo;

    // Valida Merkle Roots
    public async Task ValidateMerkleRootsAsync(CancellationToken cancellationToken = default)
    {
        var invoices = await _invoiceRepo.GetAllAsync(cancellationToken);
        foreach (var invoice in invoices)
        {
            if (!string.IsNullOrEmpty(invoice.MerkleRoot))
            {
                var isValid = await _blockchainIntegrationService.ValidateMerkleRootAsync(invoice.MerkleRoot, invoice.DueDate, cancellationToken);
                if (!isValid)
                {
                    await _auditLogRepository.AddAsync(new AuditLog
                    {
                        Action = "Inconsistencia Merkle Root",
                        EntityName = nameof(Invoice),
                        EntityId = invoice.Id.ToString(),
                        UserName = "system",
                        Details = $"Diferencia entre Merkle Root en DB ({invoice.MerkleRoot}) y Blockchain.",
                        Timestamp = DateTime.UtcNow
                    }, cancellationToken);
                }
            }
        }
    }

    // Valida splits registrados
    public async Task ValidateSplitsAsync(CancellationToken cancellationToken = default)
    {
        var units = await _unitAccountRepo.GetAllAsync(cancellationToken);
        foreach (var unit in units)
        {
            // Busca transacciones tipo Split
            var splits = await _transactionRepo.GetSplitsByUnitIdAsync(unit.Id, cancellationToken);
            foreach (var split in splits)
            {
                // Suponemos que el split tiene los montos operativo y reserva
                // y que Transaction tiene campos MontoOperativo y MontoReserva
                var result = await _blockchainIntegrationService.RegisterSplitOnChainAsync(
                    unit.ExternalUnitId, split.MontoOperativo ?? 0, split.MontoReserva ?? 0, cancellationToken);
                if (!result.Success)
                {
                    await _auditLogRepository.AddAsync(new AuditLog
                    {
                        Action = "Inconsistencia Split Blockchain",
                        EntityName = "Split",
                        EntityId = split.Id.ToString(),
                        UserName = "system",
                        Details = $"No se pudo validar split en blockchain. Error: {result.ErrorMessage}",
                        Timestamp = DateTime.UtcNow
                    }, cancellationToken);
                }
            }
        }
    }

    // Puedes agregar métodos similares para pagos y fondos globales
}
