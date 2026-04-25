namespace CondoNet.Financial.Core.Entities
{
    public class Invoice : BaseEntity
    {
        public Guid UnitAccountId { get; set; }
        public string FolioNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public InvoiceStatus Status { get; set; } // Pending, Paid, PartiallyPaid
        public DateTime DueDate { get; set; }

        // Seguridad Blockchain
        public string? MerkleRoot { get; set; } // El hash que valida este grupo de gastos
        public string? BlockchainRecordId { get; set; } // Referencia al bloque/contrato

        public virtual UnitAccount UnitAccount { get; set; } = null!;
    }

    public enum InvoiceStatus { Pending, Paid, PartiallyPaid, Cancelled }
}
