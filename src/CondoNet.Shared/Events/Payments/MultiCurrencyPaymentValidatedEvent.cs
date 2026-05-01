namespace CondoNet.Shared.Events.Payments
{
    public record MultiCurrencyPaymentValidatedEvent
    {
        public Guid PaymentId { get; init; }
        public List<CurrencyDetail> Breakdown { get; init; } = new();
        public decimal TotalUSDValue { get; init; }
        public bool IsFullPayment { get; init; }
    }

    public record CurrencyDetail(string Currency, decimal Amount, decimal Rate);
}
