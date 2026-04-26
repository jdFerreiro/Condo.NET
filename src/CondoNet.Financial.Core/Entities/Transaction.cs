namespace CondoNet.Financial.Core.Entities
{
    public class Transaction : BaseEntity
    {
        public Guid UnitAccountId { get; set; }
        public virtual UnitAccount UnitAccount { get; set; } = null!;

        public Guid? PaymentId { get; set; }
        public virtual Payment? Payment { get; set; }

        public Guid? InvoiceId { get; set; }
        public virtual Invoice? Invoice { get; set; }

        public Guid? CondoExpenseId { get; set; }
        public virtual CondoExpense? CondoExpense { get; set; }

        public decimal Amount { get; set; }

        // Campos para splits multicomponente
        public decimal? MontoOperativo { get; set; }
        public decimal? MontoReserva { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public TransactionType Type { get; set; }

        // Blockchain
        public string? BlockchainTxHash { get; set; }
        public string? MerkleRoot { get; set; }
    }

    public enum TransactionType
    {
        Credit = 1, // Ingreso
        Debit = 2,  // Egreso (Pago/Gasto)
        Split = 3,  // Distribución automática
        Adjustment = 4 // Ajuste manual o automático
    }
}
