namespace CondoNet.Financial.Core.Entities
{
    public class BankTransaction : BaseEntity
    {
        public string Reference { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string OriginPhone { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public bool IsMatched { get; set; }
    }
}
