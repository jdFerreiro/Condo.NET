namespace CondoNet.Payment.Infrastructure.PaymentGateways;

public class PaypalOptions
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}

public class PaypalPaymentGatewayService : IPaymentGatewayService
{
    public string GatewayName => "Paypal";
    private readonly PaypalOptions _options;
    public PaypalPaymentGatewayService(PaypalOptions options)
    {
        _options = options;
    }
    public Task<bool> ProcessPaymentAsync(PaymentGatewayRequest request, CancellationToken cancellationToken = default)
    {
        // Implementación real con Paypal SDK
        return Task.FromResult(true);
    }
    public Task<bool> ValidateWebhookAsync(string payload, CancellationToken cancellationToken = default)
    {
        // Implementación real con Paypal SDK
        return Task.FromResult(true);
    }
}
