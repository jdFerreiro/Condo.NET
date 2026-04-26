namespace CondoNet.Shared.DTOs.Financial
{
    public class RegisterPaymentDto
    {
        public string UnitId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string TransactionReference { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;

        // Soporte multimoneda
        public decimal AmountUSD { get; set; }
        public decimal AmountVES { get; set; }
    }
}
