namespace CondoNet.Payment.Core.Services;

using CondoNet.Payment.Core.Models;

public interface IPaymentReceiptValidator
{
    Task<bool> ValidateAsync(PaymentReceiptValidationRequest request, CancellationToken ct = default);
}
