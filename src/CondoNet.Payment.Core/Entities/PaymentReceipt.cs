namespace CondoNet.Payment.Core.Entities
{
    public class PaymentReceipt
    {
        public Guid Id { get; set; }
        public Guid PaymentId { get; set; }
        public string ReceiptNumber { get; set; } = null!;
        public DateTime IssuedAt { get; set; }
        public Payment? Payment { get; set; }
    }
}