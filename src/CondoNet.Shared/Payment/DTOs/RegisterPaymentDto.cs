namespace CondoNet.Shared.Payment.DTOs
{
    public class RegisterPaymentDto
    {
        public string UnitId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string TransactionReference { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;

        // Soporte multimoneda combinado
        public List<PaymentDetailDto> PaymentDetails { get; set; } = [];
    }
}
