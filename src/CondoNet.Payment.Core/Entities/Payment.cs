namespace CondoNet.Payment.Core.Entities
{
    public class Payment
    {
        public Guid Id { get; set; }
        public Guid InvoiceId { get; set; }
        public Guid PaymentGatewayId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaidAt { get; set; }
        public string Status { get; set; } = null!;
        public Invoice? Invoice { get; set; }
        public PaymentGateway? PaymentGateway { get; set; }
        public PaymentReceipt? Receipt { get; set; }
    }
}