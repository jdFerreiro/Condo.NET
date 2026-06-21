namespace CondoNet.Shared.Events.Payments
{
    public class InvoicePaidEvent
    {
        public Guid CorrelationId { get; init; }
        public Guid InvoiceId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaidAt { get; set; }
        public string PaymentReference { get; set; } = string.Empty;
        public string Gateway { get; set; } = string.Empty;
    }
}
