namespace CondoNet.Financial.Core.Entities
{
    public class FundBalance : BaseEntity
    {
        public decimal BalanceVES { get; set; }
        public decimal BalanceUSD { get; set; }
        public DateTime LastRevaluationDate { get; set; }
        public ICollection<FundMovement> Movements { get; set; } = [];
    }
}
