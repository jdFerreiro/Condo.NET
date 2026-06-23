using CondoNet.Shared.Events.Payments;
using CondoNet.Accounting.Core.Entities;
using MassTransit;
using CondoNet.Accounting.Core.Interfaces.Services;

namespace CondoNet.Accounting.Infrastructure.Consumers;

public class InvoiceRegisteredEventConsumer : IConsumer<InvoiceRegisteredEvent>
{
    private readonly IAccountingAutomatonService _automatonService;

    public InvoiceRegisteredEventConsumer(IAccountingAutomatonService automatonService)
    {
        _automatonService = automatonService;
    }

    public async Task Consume(ConsumeContext<InvoiceRegisteredEvent> context)
    {
        var evt = context.Message;
        // Mapea el evento a una transacción contable de registro de obligación
        var transaction = new AccountingTransaction
        {
            Id = Guid.NewGuid(),
            Date = evt.RegisteredAt,
            Description = $"Registro de factura {evt.InvoiceId} - {evt.Description}",
            Reference = evt.Reference,
            Entries = new List<AccountingEntry>()
            {
                // Debe: Gasto (ejemplo, ajustar AccountId real)
                new AccountingEntry {
                    Id = Guid.NewGuid(),
                    AccountId = Guid.Empty, // Reemplazar por el ID real de la cuenta de gasto
                    Debit = evt.Amount,
                    Credit = 0,
                    Description = "Gasto por factura registrada"
                },
                // Haber: Cuentas por pagar (ajustar AccountId real)
                new AccountingEntry {
                    Id = Guid.NewGuid(),
                    AccountId = Guid.Empty, // Reemplazar por el ID real de la cuenta por pagar
                    Debit = 0,
                    Credit = evt.Amount,
                    Description = "Cuentas por pagar a proveedores"
                }
            }
        };
        await _automatonService.ProcessTransactionAsync(transaction, Guid.Empty);
    }
}
