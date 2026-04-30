using System.Threading;
using System.Threading.Tasks;

namespace CondoNet.Payment.Infrastructure.PaymentGateways;

public interface IPaymentGatewayService
{
    string GatewayName { get; }
    Task<bool> ProcessPaymentAsync(PaymentGatewayRequest request, CancellationToken cancellationToken = default);
    Task<bool> ValidateWebhookAsync(string payload, CancellationToken cancellationToken = default);
}

public class PaymentGatewayRequest
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public string? CustomerId { get; set; }
    public string? Description { get; set; }
}
