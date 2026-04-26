namespace CondoNet.Financial.Core.Entities
{
    public class PaymentIntent : BaseEntity
    {
        public Guid UnitAccountId { get; set; }
        public UnitAccount UnitAccount { get; set; } = null!;
        public decimal AmountUSD { get; set; }
        public decimal LockedRate { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsCompleted { get; set; }
    }
}
