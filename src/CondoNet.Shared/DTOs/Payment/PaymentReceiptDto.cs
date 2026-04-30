namespace CondoNet.Shared.DTOs.Payment;

public class PaymentReceiptDto
{
    public Guid Id { get; set; }
    public Guid PaymentId { get; set; }
    public string ReceiptNumber { get; set; } = null!;
    public DateTime IssuedAt { get; set; }
}
