using CondoNet.BlockChain.Core;
using CondoNet.Financial.Core.Entities;
using CondoNet.Financial.Core.Interfaces;
using CondoNet.Financial.Core.Services;
using CondoNet.Shared.DTOs.Financial;
using MassTransit;

namespace CondoNet.Financial.Infrastructure.Services
{
    public class FinancialService(
        IPaymentRepository paymentRepo,
        IInvoiceRepository invoiceRepo,
        IUnitAccountRepository unitAccountRepo,
        IBillingConfigurationRepository billingConfigRepo,
        ITransactionRepository transactionRepo,
        IFinancialCondominiumConfigurationRepository financialCondominiumConfigurationRepository,
        IBlockchainIntegrationService blockchainIntegrationService,
        IAuditLogRepository auditLogRepository,
        IGlobalFundRepository globalFundRepository,
        ICurrencyService currencyService,
        ICurrencyAdjustmentLogRepository currencyAdjustmentLogRepository,
        IPublishEndpoint publishEndpoint) : IFinancialService
    {
        private readonly ICurrencyService _currencyService = currencyService;
        private readonly IPaymentRepository _paymentRepo = paymentRepo;
        private readonly IInvoiceRepository _invoiceRepo = invoiceRepo;
        private readonly IUnitAccountRepository _unitAccountRepo = unitAccountRepo;
        private readonly IBillingConfigurationRepository _billingConfigRepo = billingConfigRepo;
        private readonly IAuditLogRepository _auditLogRepository = auditLogRepository;
        private readonly ITransactionRepository _transactionRepo = transactionRepo;
        private readonly IBlockchainIntegrationService _blockchainIntegrationService = blockchainIntegrationService;
        private readonly IGlobalFundRepository _globalFundRepository = globalFundRepository;
        private readonly IFinancialCondominiumConfigurationRepository _financialConfigRepo = financialCondominiumConfigurationRepository;

        private readonly ICurrencyAdjustmentLogRepository _currencyAdjustmentLogRepository = currencyAdjustmentLogRepository;
        private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

        public async Task RegisterPaymentAsync(RegisterPaymentDto payment, CancellationToken cancellationToken = default)
        {
            // 1. Validar monto
            if (payment.Amount <= 0)
                throw new InvalidOperationException("El monto del pago debe ser mayor que cero.");

            // 2. Buscar la unidad
            var unit = await _unitAccountRepo.GetByExternalUnitIdAsync(payment.UnitId, cancellationToken) ?? throw new InvalidOperationException("La unidad no existe.");

            // 2b. Consultar configuración financiera del condominio
            var config = await _financialConfigRepo.GetByCondominiumIdAsync(unit.CondominiumId, cancellationToken) ?? throw new InvalidOperationException("No hay configuración financiera para el condominio.");

            // 3. Buscar la factura del periodo (asumimos periodo = mes actual)
            var invoices = await _invoiceRepo.GetByUnitAccountIdAsync(unit.Id, cancellationToken);
            var invoice = invoices.OrderByDescending(i => i.DueDate).FirstOrDefault(i => i.Status == InvoiceStatus.Pending || i.Status == InvoiceStatus.PartiallyPaid) ?? throw new InvalidOperationException("No hay factura pendiente para la unidad.");

            // 4. Validar referencia única por unidad/periodo
            var existingPayments = await _paymentRepo.GetByInvoiceIdAsync(invoice.Id, cancellationToken);
            if (existingPayments.Any(p => p.TransactionReference == payment.TransactionReference))
                throw new InvalidOperationException("La referencia de pago ya fue utilizada para esta unidad y periodo.");

            // 5. Validar que el monto no exceda la deuda
            var deudaPendiente = invoice.TotalAmount - invoice.Payments.Sum(p => p.Amount);
            if (payment.Amount > deudaPendiente)
                throw new InvalidOperationException($"El monto del pago ({payment.Amount}) excede la deuda pendiente ({deudaPendiente}).");

            // 6. Validar límite de deuda y bloqueo
            if (config.BlockOnDebt && unit.CurrentDebt > config.DebtLimit)
                throw new InvalidOperationException($"La unidad supera el límite de deuda permitido ({config.DebtLimit}). Acceso o servicios pueden estar bloqueados.");



            // 7. Validar y aplicar diferencial cambiario y pagos multimoneda combinados
            bool isMultiCurrency = payment.PaymentDetails != null && payment.PaymentDetails.Count > 0;
            decimal montoARegistrar = 0;
            decimal fundImpact = 0;
            decimal remainingDebtUSD = 0;
            List<PaymentDetail> paymentDetails = [];

            if (isMultiCurrency)
            {
                decimal totalUSD = 0;
                foreach (var detail in payment.PaymentDetails!)
                {
                    // Obtener la tasa vigente para la moneda
                    var rate = detail.Currency == "USD" ? 1m : (await _currencyService.GetActiveRateAsync(detail.Currency, unit.OrganizationId, unit.CondominiumId, cancellationToken))?.Rate ?? 0m;
                    if (rate == 0 && detail.Currency != "USD")
                        throw new InvalidOperationException($"No hay tasa activa para {detail.Currency}");

                    // Convertir a USD para validación
                    decimal usdValue = detail.Currency == "USD" ? detail.Amount : detail.Amount / rate;
                    totalUSD += usdValue;

                    paymentDetails.Add(new PaymentDetail
                    {
                        Currency = detail.Currency,
                        Amount = detail.Amount,
                        RateAtMoment = rate,
                        Method = detail.Method
                    });
                }

                montoARegistrar = payment.Amount > 0 ? payment.Amount : paymentDetails.Sum(x => x.Amount); // Monto total reportado

                // Validar diferencial cambiario si está habilitado
                if (config.EnableCurrencyDifferential)
                {
                    var expectedUSD = deudaPendiente; // Deuda en USD
                    decimal tolerance = expectedUSD * 0.005m;
                    if (totalUSD >= (expectedUSD - tolerance))
                    {
                        fundImpact = totalUSD - expectedUSD;
                    }
                    else
                    {
                        remainingDebtUSD = expectedUSD - totalUSD;
                    }

                    if (fundImpact != 0 || remainingDebtUSD > 0)
                    {
                        await _currencyAdjustmentLogRepository.AddAsync(new CurrencyAdjustmentLog
                        {
                            OrganizationId = unit.OrganizationId,
                            EntityType = EntityType.Unit,
                            EntityId = unit.Id,
                            ReferenceInvoiceId = invoice.Id,
                            PreviousRate = 0,
                            NewRate = 0,
                            AmountVesDiff = fundImpact,
                            Reason = CurrencyAdjustmentReason.PaymentDifferential,
                            CreatedAt = DateTime.UtcNow
                        }, cancellationToken);

                        var fund = await _globalFundRepository.GetByTypeAsync(GlobalFundType.Differential, cancellationToken);
                        if (fund != null)
                        {
                            fund.Balance += fundImpact;
                            await _globalFundRepository.UpdateAsync(fund, cancellationToken);
                        }

                        if (remainingDebtUSD > 0)
                        {
                            unit.CurrentDebt += remainingDebtUSD;
                            await _unitAccountRepo.UpdateAsync(unit, cancellationToken);
                        }
                    }
                }
            }
            else
            {
                montoARegistrar = payment.Amount;
            }

            // 8. Registrar el pago (pendiente de certificación)
            var paymentEntity = new Payment
            {
                InvoiceId = invoice.Id,
                Invoice = invoice,
                Amount = montoARegistrar,
                Method = Enum.TryParse<PaymentMethod>(payment.PaymentMethod, out var method) ? method : PaymentMethod.Transfer,
                TransactionReference = payment.TransactionReference,
                IsConfirmedOnChain = false,
                PaymentDetails = paymentDetails
            };
            await _paymentRepo.AddAsync(paymentEntity, cancellationToken);

            // Publicar evento PaymentValidatedEvent (para pagos simples)
            if (!isMultiCurrency)
            {
                await _publishEndpoint.Publish(new Shared.Events.Payments.PaymentValidatedEvent
                {
                    PaymentId = paymentEntity.Id,
                    OrganizationId = unit.OrganizationId,
                    UnitId = unit.Id,
                    AmountUSD = montoARegistrar, // Ajusta si tienes el monto en USD
                    AmountVES = montoARegistrar, // Ajusta si tienes el monto en VES
                    ExchangeRateApplied = 0, // Ajusta según la tasa usada
                    ValidationTimestamp = DateTime.UtcNow,
                    IsBalanceCleared = (remainingDebtUSD == 0),
                    CorrelationId = Guid.NewGuid().ToString()
                }, cancellationToken);
            }

            // 9. Registrar la transacción de pago en blockchain
            var blockchainResult = await _blockchainIntegrationService.RegisterPaymentOnChainAsync(payment, cancellationToken);
            if (!blockchainResult.Success)
            {
                // Registrar log de error en auditoría
                await _auditLogRepository.AddAsync(new AuditLog
                {
                    Action = "Error al registrar pago en blockchain",
                    EntityName = nameof(Payment),
                    EntityId = paymentEntity.Id.ToString(),
                    UserName = "system", // Reemplazar por usuario real si está disponible
                    Details = $"Referencia: {payment.TransactionReference}, Error: {blockchainResult.ErrorMessage}",
                    Timestamp = DateTime.UtcNow
                }, cancellationToken);
                throw new InvalidOperationException($"Error al registrar el pago en blockchain: {blockchainResult.ErrorMessage}");
            }
        }

        public async Task<AccountStatusDto> GetAccountStatusAsync(string unitId, CancellationToken cancellationToken = default)
        {
            // 1. Buscar la unidad
            var unit = await _unitAccountRepo.GetByExternalUnitIdAsync(unitId, cancellationToken) ?? throw new InvalidOperationException("La unidad no existe.");

            // 2. Buscar la última factura
            var invoices = await _invoiceRepo.GetByUnitAccountIdAsync(unit.Id, cancellationToken);
            var lastInvoice = invoices.OrderByDescending(i => i.DueDate).FirstOrDefault();

            // 3. Obtener el Merkle Root del último periodo facturado
            string merkleRoot = lastInvoice?.MerkleRoot ?? string.Empty;
            DateTime lastUpdate = lastInvoice?.DueDate ?? DateTime.MinValue;

            // 4. Validar Merkle Root en blockchain y auditar inconsistencias
            if (!string.IsNullOrEmpty(merkleRoot) && lastInvoice != null)
            {
                var isValid = await _blockchainIntegrationService.ValidateMerkleRootAsync(merkleRoot, lastInvoice.DueDate, cancellationToken);
                if (!isValid)
                {
                    await _auditLogRepository.AddAsync(new AuditLog
                    {
                        Action = "Inconsistencia Merkle Root",
                        EntityName = nameof(Invoice),
                        EntityId = lastInvoice.Id.ToString(),
                        UserName = "system",
                        Details = $"Diferencia entre Merkle Root en DB ({merkleRoot}) y Blockchain.",
                        Timestamp = DateTime.UtcNow
                    }, cancellationToken);
                }
            }

            // 5. Mapear a DTO
            return new AccountStatusDto
            {
                UnitId = unit.ExternalUnitId,
                CurrentDebt = unit.CurrentDebt,
                CreditBalance = unit.CreditBalance,
                LastUpdate = lastUpdate,
                LastExpenseMerkleRoot = merkleRoot
            };
        }


        public async Task SplitFundsAsync(RegisterPaymentDto payment, CancellationToken cancellationToken = default)
        {
            // 1. Buscar la unidad
            var unit = await _unitAccountRepo.GetByExternalUnitIdAsync(payment.UnitId, cancellationToken) ?? throw new InvalidOperationException("La unidad no existe.");

            // 2. Obtener configuración financiera del condominio
            var config = await _financialConfigRepo.GetByCondominiumIdAsync(unit.CondominiumId, cancellationToken) ?? throw new InvalidOperationException("No hay configuración financiera para el condominio.");
            var reservePct = config.ReserveFundPercentage / 100m;
            //decimal operativoPct = 1m - reservePct;

            // 3. Calcular montos
            var montoReserva = Math.Round(payment.Amount * reservePct, 2);
            var montoOperativo = payment.Amount - montoReserva;


            // 4. Registrar transacciones
            await _transactionRepo.AddAsync(new Transaction
            {
                UnitAccountId = unit.Id,
                Amount = montoOperativo,
                TransactionDate = DateTime.UtcNow,
                Description = "Ingreso operativo por pago registrado",
                Type = TransactionType.Credit
            }, cancellationToken);

            await _transactionRepo.AddAsync(new Transaction
            {
                UnitAccountId = unit.Id,
                Amount = montoReserva,
                TransactionDate = DateTime.UtcNow,
                Description = "Ingreso a fondo de reserva por pago registrado",
                Type = TransactionType.Split
            }, cancellationToken);

            // 4b. Registrar split en blockchain
            var splitResult = await _blockchainIntegrationService.RegisterSplitOnChainAsync(
                payment.UnitId, montoOperativo, montoReserva, cancellationToken);
            if (!splitResult.Success)
            {
                await _auditLogRepository.AddAsync(new AuditLog
                {
                    Action = "Error al registrar split en blockchain",
                    EntityName = "Split",
                    EntityId = unit.Id.ToString(),
                    UserName = "system",
                    Details = $"Error: {splitResult.ErrorMessage}",
                    Timestamp = DateTime.UtcNow
                }, cancellationToken);
            }

            // 5. Actualizar saldos globales de fondos
            var operatingFund = await _globalFundRepository.GetByTypeAsync(GlobalFundType.Operating, cancellationToken);
            if (operatingFund != null)
            {
                operatingFund.Balance += montoOperativo;
                await _globalFundRepository.UpdateAsync(operatingFund, cancellationToken);
            }
            var reserveFund = await _globalFundRepository.GetByTypeAsync(GlobalFundType.Reserve, cancellationToken);
            if (reserveFund != null)
            {
                reserveFund.Balance += montoReserva;
                await _globalFundRepository.UpdateAsync(reserveFund, cancellationToken);
            }
        }

        /// <summary>
        /// Certifica un pago: lo marca como confirmado, ejecuta el split y actualiza saldos y crédito de la unidad.
        /// </summary>
        public async Task CertifyPaymentAsync(Guid paymentId, CancellationToken cancellationToken = default)
        {
            var payment = await _paymentRepo.GetByIdAsync(paymentId, cancellationToken) ?? throw new InvalidOperationException("El pago no existe.");
            if (payment.IsConfirmedOnChain)
                throw new InvalidOperationException("El pago ya está certificado.");

            var invoice = await _invoiceRepo.GetByIdAsync(payment.InvoiceId, cancellationToken) ?? throw new InvalidOperationException("La factura asociada no existe.");
            var unit = await _unitAccountRepo.GetByIdAsync(invoice.UnitAccountId, cancellationToken) ?? throw new InvalidOperationException("La unidad asociada no existe.");

            // Ejecutar split automático
            var dto = new RegisterPaymentDto
            {
                UnitId = unit.ExternalUnitId,
                Amount = payment.Amount,
                TransactionReference = payment.TransactionReference,
                PaymentDate = payment.CreatedAt,
                PaymentMethod = payment.Method.ToString()
            };
            await SplitFundsAsync(dto, cancellationToken);

            // Actualizar saldo y crédito de la unidad
            var deudaPendiente = invoice.TotalAmount - invoice.Payments.Where(p => p.IsConfirmedOnChain || p.Id == payment.Id).Sum(p => p.Amount);
            if (payment.Amount >= deudaPendiente)
            {
                unit.CurrentDebt = 0;
                unit.CreditBalance += (payment.Amount - deudaPendiente);
            }
            else
            {
                unit.CurrentDebt -= payment.Amount;
            }
            await _unitAccountRepo.UpdateAsync(unit, cancellationToken);

            // Actualizar estado de la factura
            payment.IsConfirmedOnChain = true;
            await _paymentRepo.UpdateAsync(payment, cancellationToken);

            invoice.Payments.Add(payment);
            var totalPagado = invoice.Payments.Where(p => p.IsConfirmedOnChain).Sum(p => p.Amount);
            if (totalPagado >= invoice.TotalAmount)
                invoice.Status = InvoiceStatus.Paid;
            else if (totalPagado > 0)
                invoice.Status = InvoiceStatus.PartiallyPaid;
            await _invoiceRepo.UpdateAsync(invoice, cancellationToken);

            // Registrar auditoría
            await _auditLogRepository.AddAsync(new AuditLog
            {
                Action = "Certificación de pago",
                EntityName = nameof(Payment),
                EntityId = payment.Id.ToString(),
                UserName = "system", // Reemplazar por usuario real si está disponible
                Details = $"Pago certificado y saldos actualizados. Monto: {payment.Amount}, Unidad: {unit.ExternalUnitId}",
                Timestamp = DateTime.UtcNow
            }, cancellationToken);
        }
        public async Task<PaymentValidationResult> ValidatePaymentWithCurrencyDiffAsync(
            string unitId, decimal amountUSD, decimal amountVES, CancellationToken cancellationToken = default)
        {
            var unit = await _unitAccountRepo.GetByExternalUnitIdAsync(unitId, cancellationToken)
                ?? throw new InvalidOperationException("La unidad no existe.");

            //var config = await _financialConfigRepo.GetByCondominiumIdAsync(unit.CondominiumId, cancellationToken)
            //    ?? throw new InvalidOperationException("No hay configuración financiera para el condominio.");

            var rate = await _currencyService.GetActiveRateAsync("VES", unit.OrganizationId, unit.CondominiumId, cancellationToken) ?? throw new InvalidOperationException("No hay tasa de cambio activa para VES.");
            decimal expectedVES = amountUSD * rate.Rate;
            decimal tolerance = expectedVES * 0.005m;

            if (amountVES >= (expectedVES - tolerance))
            {
                return new PaymentValidationResult
                {
                    Status = "CONFIRMED",
                    AdjustmentRequired = false,
                    FundImpact = amountVES - expectedVES,
                    RemainingDebtUSD = 0
                };
            }
            else
            {
                return new PaymentValidationResult
                {
                    Status = "PARTIAL_CONFIRMED",
                    AdjustmentRequired = true,
                    FundImpact = 0,
                    RemainingDebtUSD = (expectedVES - amountVES) / rate.Rate,
                    Message = "El monto transferido no cubre la totalidad debido al cambio de tasa."
                };
            }
        }
    }
}
