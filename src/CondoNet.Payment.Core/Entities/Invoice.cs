namespace CondoNet.Payment.Core.Entities
{
    public class Invoice
    {
        public Guid Id { get; set; }
        public string Number { get; set; } = null!;
        public decimal Amount { get; set; }
        public DateTime IssuedAt { get; set; }
        public DateTime? DueAt { get; set; }
        public ICollection<Payment>? Payments { get; set; }
    }
}