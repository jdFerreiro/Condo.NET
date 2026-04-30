namespace CondoNet.Payment.Infrastructure.PaymentGateways;

public class MercadoPagoOptions
{
    public string AccessToken { get; set; } = string.Empty;
    public string PublicKey { get; set; } = string.Empty;
}

public class MercadoPagoPaymentGatewayService : IPaymentGatewayService
{
    public string GatewayName => "MercadoPago";
    private readonly MercadoPagoOptions _options;
    public MercadoPagoPaymentGatewayService(MercadoPagoOptions options)
    {
        _options = options;
    }
    public Task<bool> ProcessPaymentAsync(PaymentGatewayRequest request, CancellationToken cancellationToken = default)
    {
        // Implementación real con MercadoPago SDK
        return Task.FromResult(true);
    }
    public Task<bool> ValidateWebhookAsync(string payload, CancellationToken cancellationToken = default)
    {
        // Implementación real con MercadoPago SDK
        return Task.FromResult(true);
    }
}
