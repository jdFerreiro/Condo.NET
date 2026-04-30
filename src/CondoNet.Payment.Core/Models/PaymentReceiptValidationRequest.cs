using System;

namespace CondoNet.Payment.Core.Models;

public class PaymentReceiptValidationRequest
{
    public string ReceiptNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Reference { get; set; }
    public string? Gateway { get; set; }
    public DateTime? PaidAt { get; set; }
}
