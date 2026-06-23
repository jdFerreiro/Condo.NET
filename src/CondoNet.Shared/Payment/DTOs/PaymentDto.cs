namespace CondoNet.Shared.Payment.DTOs;

public class PaymentDto
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public Guid PaymentGatewayId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaidAt { get; set; }
    public string Status { get; set; } = null!;
    public string? ReceiptNumber { get; set; }
}
