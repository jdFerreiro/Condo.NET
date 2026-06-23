namespace CondoNet.Accounting.Core.Entities
{
    public class AccountingTransaction
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid CondominiumId { get; set; }

        public string Number { get; set; } = null!; // Número de comprobante correlativo automático
        public string Description { get; set; } = null!;
        public DateTime Date { get; set; }
        public TransactionStatus Status { get; set; } = TransactionStatus.Posted;
        public string CreatedBy { get; set; } = null!;

        // Un comprobante tiene N renglones o apuntes (Mínimo 2 para cumplir Partida Doble)
        public List<AccountingEntry> Entries { get; set; } = [];
    }

    public enum TransactionStatus { Draft = 1, Posted = 2, Voided = 3 }
}
