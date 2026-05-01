using CondoNet.Financial.Core.Interfaces;
using CondoNet.Shared.Events.Payments;
using MassTransit;

namespace CondoNet.Financial.Infrastructure.Consumers
{
    public class InvoicePaidConsumer : IConsumer<InvoicePaidEvent>
    {
        private readonly IInvoiceRepository _invoiceRepository;
        public InvoicePaidConsumer(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public async Task Consume(ConsumeContext<InvoicePaidEvent> context)
        {
            var evt = context.Message;
            var invoice = await _invoiceRepository.GetByIdAsync(evt.InvoiceId);
            if (invoice != null)
            {
                invoice.Status = Core.Entities.InvoiceStatus.Paid;
                invoice.UpdatedAt = evt.PaidAt;
                // invoice.PaymentReference = evt.PaymentReference;
                await _invoiceRepository.UpdateAsync(invoice);
            }
            // Si no existe, podrías loggear o manejar el error
        }
    }
}
