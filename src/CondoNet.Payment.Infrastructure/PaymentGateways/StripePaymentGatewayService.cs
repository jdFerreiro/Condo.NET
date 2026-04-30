namespace CondoNet.Payment.Infrastructure.PaymentGateways;

public class StripeOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
}

public class StripePaymentGatewayService : IPaymentGatewayService
{
    public string GatewayName => "Stripe";
    private readonly StripeOptions _options;
    public StripePaymentGatewayService(StripeOptions options)
    {
        _options = options;
    }
    public Task<bool> ProcessPaymentAsync(PaymentGatewayRequest request, CancellationToken cancellationToken = default)
    {
        // Implementación real con Stripe SDK
        return Task.FromResult(true);
    }
    public Task<bool> ValidateWebhookAsync(string payload, CancellationToken cancellationToken = default)
    {
        // Implementación real con Stripe SDK
        return Task.FromResult(true);
    }
}
