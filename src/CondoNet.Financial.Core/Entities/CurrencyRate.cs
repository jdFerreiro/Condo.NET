namespace CondoNet.Financial.Core.Entities
{
    public class CurrencyRate : BaseEntity
    {
        public string CurrencyIsoCode { get; set; } = "VES";
        public decimal Rate { get; set; }
        public DateTime Date { get; set; }
        public string Source { get; set; } = "BCV";
        public bool IsActive { get; set; }
    }
}
