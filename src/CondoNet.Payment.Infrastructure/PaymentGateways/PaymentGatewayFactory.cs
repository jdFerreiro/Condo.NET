namespace CondoNet.Payment.Infrastructure.PaymentGateways;

public class PaymentGatewayFactory
{
    private readonly IDictionary<string, IPaymentGatewayService> _gateways;

    public PaymentGatewayFactory(IEnumerable<IPaymentGatewayService> gateways)
    {
        _gateways = new Dictionary<string, IPaymentGatewayService>(StringComparer.OrdinalIgnoreCase);
        foreach (var gateway in gateways)
        {
            _gateways[gateway.GatewayName] = gateway;
        }
    }

    public IPaymentGatewayService GetGateway(string gatewayName)
    {
        if (_gateways.TryGetValue(gatewayName, out var service))
            return service;
        throw new KeyNotFoundException($"Payment gateway '{gatewayName}' not registered.");
    }
}
