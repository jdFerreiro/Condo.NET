namespace CondoNet.Financial.Core.Entities
{
    public class Invoice : BaseEntity
    {
        public string Number { get; set; } = null!; // Folio correlativo
        public Guid UnitAccountId { get; set; }
        public virtual UnitAccount UnitAccount { get; set; } = null!;

        public decimal TotalAmount { get; set; }
        public DateTime DueDate { get; set; }
        public InvoiceStatus Status { get; set; }

        // Emisión de documento digital
        public DateTime? FechaEmision { get; set; }
        public bool IsEmitted { get; set; }

        // Trazabilidad Blockchain
        public string? MerkleRoot { get; set; }
        public string? BlockchainTxHash { get; set; }

        public virtual ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
    public enum InvoiceStatus { Pending, Paid, PartiallyPaid, Cancelled }
}
