using CondoNet.Asset.Core.Entities;
using CondoNet.Asset.Infraestructure.Persistence;
using CondoNet.Assets.API.Mappings;
using CondoNet.Shared.Asset.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Assets.Workers;

public class BulkImportConsumer : IConsumer<ProcessBulkImportCommand>
{
    private readonly AssetDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<BulkImportConsumer> _logger;

    public BulkImportConsumer(
        AssetDbContext context,
        IPublishEndpoint publishEndpoint,
        ILogger<BulkImportConsumer> logger)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProcessBulkImportCommand> context)
    {
        var message = context.Message;
        _logger.LogInformation("Iniciando procesamiento de carga diferida {ImportId}", message.ImportId);

        // 1. Notificar Inicio
        await _publishEndpoint.Publish(new ImportStarted(message.ImportId, message.OrganizationId));

        // 2. Validación de la Regla de Oro (100%)
        decimal totalAliquot = message.Units.Sum(u => u.Aliquot);
        if (totalAliquot != 100.0000m)
        {
            var error = $"La suma de alícuotas es {totalAliquot}%. Debe ser exactamente 100.0000%.";
            _logger.LogWarning("Carga {ImportId} fallida: {Error}", message.ImportId, error);

            await _publishEndpoint.Publish(new ImportFailed(message.ImportId, message.OrganizationId, error));
            return;
        }

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // 3. Procesar Torres
            var towerNames = message.Units.Select(u => u.TowerName).Distinct();
            var towers = new List<Tower>();

            foreach (var name in towerNames)
            {
                var tower = await _context.Towers
                    .FirstOrDefaultAsync(t => t.Name == name && t.CondominiumId == message.CondominiumId);

                if (tower == null)
                {
                    tower = new Tower { Name = name, CondominiumId = message.CondominiumId };
                    _context.Towers.Add(tower);
                    await _context.SaveChangesAsync();
                }
                towers.Add(tower);
            }

            // 4. Crear Unidades
            //var unitsToInsert = message.Units.Select(dto => new Unit
            //{
            //    Identifier = dto.Identifier,
            //    Aliquot = dto.Aliquot,
            //    Type = dto.Type,
            //    OwnerEmail = dto.OwnerEmail,
            //    TowerId = towers.First(t => t.Name == dto.TowerName).Id,
            //    OrganizationId = message.OrganizationId // Asignación manual requerida en Background Worker
            //}).ToList();

            var unitsToInsert = message.Units.Select(dto =>
                dto.ToEntity(
                    message.OrganizationId,
                    towers.First(t => t.Name == dto.TowerName).Id
                )
            ).ToList();

            await _context.Units.AddRangeAsync(unitsToInsert);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Carga {ImportId} completada exitosamente.", message.ImportId);

            // 5. Notificar Éxito
            await _publishEndpoint.Publish(new ImportCompleted(message.ImportId, message.OrganizationId, unitsToInsert.Count));

            // 6. Emitir evento de integración para otros servicios (Billing, etc.)
            var eventUnits = unitsToInsert.Select(u => new ImportedUnitDto(u.Id, u.Identifier, u.Aliquot, u.OwnerEmail)).ToList();
            await _publishEndpoint.Publish(new UnitsImported(message.CondominiumId, message.OrganizationId, eventUnits));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error crítico al procesar la carga {ImportId}", message.ImportId);

            // Notificar Error
            await _publishEndpoint.Publish(new ImportFailed(message.ImportId, message.OrganizationId, ex.Message));
        }
    }
}
