namespace CondoNet.Payment.Core.Entities
{
    public class PaymentGateway
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Provider { get; set; } = null!;
        public string Configuration { get; set; } = null!;
        public ICollection<Payment>? Payments { get; set; }
    }
}