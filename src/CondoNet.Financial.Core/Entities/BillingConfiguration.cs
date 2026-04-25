namespace CondoNet.Financial.Core.Entities
{
    public class BillingConfiguration : BaseEntity
    {
        public string ContractAddress { get; set; } = null!; // Dirección del Smart Contract
        public decimal ReserveFundPercentage { get; set; }
        public decimal LateFeeInterest { get; set; }
        public CurrencyCode CurrencyCode { get; set; } = CurrencyCode.USD;
    }

    public enum CurrencyCode { USD, EUR, GBP, JPY, BTC, ETH, VES }
}
