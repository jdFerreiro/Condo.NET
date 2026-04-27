namespace CondoNet.Financial.Core.Entities
{

    public class Payment : BaseEntity
    {
        public Guid InvoiceId { get; set; }
        public virtual Invoice Invoice { get; set; } = null!;

        public decimal Amount { get; set; }
        public PaymentMethod Method { get; set; } // Cash, Transfer, Crypto
        public string TransactionReference { get; set; } = null!; // ID Transacción bancaria o Hash
        public bool IsConfirmedOnChain { get; set; }

        // Multimoneda
        public ICollection<PaymentDetail> PaymentDetails { get; set; } = [];
    }

    public enum PaymentMethod { Transfer, CreditCard, Crypto, Cash }
}
