namespace CondoNet.Shared.Payment.DTOs;

public class PaymentDetailDto
{
    public string Currency { get; set; } = string.Empty; // "USD", "VES", "EUR"
    public decimal Amount { get; set; }
    public decimal RateAtMoment { get; set; }
    public string Method { get; set; } = string.Empty; // Transferencia, Zelle, etc.
}
