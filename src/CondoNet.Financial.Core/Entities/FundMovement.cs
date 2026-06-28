namespace CondoNet.Financial.Core.Entities
{
    public class FundMovement : BaseEntity
    {
        public Guid FundBalanceId { get; set; }
        public FundBalance FundBalance { get; set; } = null!;
        public Guid? AdjustmentId { get; set; } // FK a CurrencyAdjustmentLog
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
