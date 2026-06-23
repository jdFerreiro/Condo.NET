namespace CondoNet.Accounting.Core.Entities
{
    public class AccountingEntry
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid CondominiumId { get; set; }

        public Guid AccountingTransactionId { get; set; }
        public Guid AccountId { get; set; }

        public decimal Debit { get; set; }  // Debe
        public decimal Credit { get; set; } // Haber
        public string? Reference { get; set; } // Notas auxiliares por renglón

        public Account Account { get; set; } = null!;
        public AccountingTransaction Transaction { get; set; } = null!;
    }
}
