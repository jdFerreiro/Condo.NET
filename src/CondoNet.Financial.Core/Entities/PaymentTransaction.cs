namespace CondoNet.Financial.Core.Entities
{
    public class PaymentTransaction : BaseEntity
    {
        public Guid PaymentIntentId { get; set; }
        public PaymentIntent PaymentIntent { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public decimal RateAtMoment { get; set; }
        public string Method { get; set; } = string.Empty; // Transferencia, Zelle, etc.
        public DateTime TransactionDate { get; set; }
    }
}
