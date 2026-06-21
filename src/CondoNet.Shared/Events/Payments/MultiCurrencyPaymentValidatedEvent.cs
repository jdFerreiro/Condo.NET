namespace CondoNet.Shared.Events.Payments
{
    public record MultiCurrencyPaymentValidatedEvent
    {
        public Guid CorrelationId { get; init; }
        public Guid PaymentId { get; set; }
        public List<CurrencyDetail> Breakdown { get; set; } = new();
        public decimal TotalUSDValue { get; set; }
        public bool IsFullPayment { get; set; }
    }

    public record CurrencyDetail
    {
        public string Currency { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal Rate { get; set; }
    };
}
