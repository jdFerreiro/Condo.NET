using CondoNet.Shared.Events.Payments;
using CondoNet.Accounting.Core.Entities;
using MassTransit;
using CondoNet.Accounting.Core.Interfaces.Services;

namespace CondoNet.Accounting.Infrastructure.Consumers;

public class InvoicePaidEventConsumer : IConsumer<InvoicePaidEvent>
{
    private readonly IAccountingAutomatonService _automatonService;

    public InvoicePaidEventConsumer(IAccountingAutomatonService automatonService)
    {
        _automatonService = automatonService;
    }

    public async Task Consume(ConsumeContext<InvoicePaidEvent> context)
    {
        var evt = context.Message;
        // Aquí deberías mapear el evento a una transacción contable
        var transaction = new AccountingTransaction
        {
            Id = Guid.NewGuid(),
            Date = evt.PaidAt,
            Description = $"Pago de factura {evt.InvoiceId} por {evt.Amount}",
            Reference = evt.PaymentReference,
            Entries = new List<AccountingEntry>()
            {
                // Ejemplo: Cargo a cuenta bancaria, abono a cuentas por cobrar
                // Estos valores deben ajustarse según el catálogo y reglas contables
                new AccountingEntry {
                    Id = Guid.NewGuid(),
                    AccountId = Guid.Empty, // Debe resolverse según configuración
                    Debit = evt.Amount,
                    Credit = 0,
                    Description = "Ingreso por pago"
                },
                new AccountingEntry {
                    Id = Guid.NewGuid(),
                    AccountId = Guid.Empty, // Debe resolverse según configuración
                    Debit = 0,
                    Credit = evt.Amount,
                    Description = "Cuentas por cobrar"
                }
            }
        };
        // El userId puede ser un sistema o el usuario del evento
        await _automatonService.ProcessTransactionAsync(transaction, Guid.Empty);
    }
}
