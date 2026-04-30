using CondoNet.Payment.Core.Models;
using CondoNet.Payment.Core.Repositories;
using CondoNet.Payment.Core.Services;

namespace CondoNet.Payment.Infrastructure.Services;

public class PaymentReceiptValidator : IPaymentReceiptValidator
{
    private readonly IPaymentReceiptRepository _receiptRepo;
    public PaymentReceiptValidator(IPaymentReceiptRepository receiptRepo)
    {
        _receiptRepo = receiptRepo;
    }

    public async Task<bool> ValidateAsync(PaymentReceiptValidationRequest request, CancellationToken ct = default)
    {
        // Ejemplo: validar unicidad y monto
        var existing = await _receiptRepo.GetByIdAsync(Guid.TryParse(request.ReceiptNumber, out var id) ? id : Guid.Empty);
        if (existing != null)
            return false; // Ya existe
        if (request.Amount <= 0)
            return false;
        // Agrega más reglas según tu lógica
        return true;
    }
}
