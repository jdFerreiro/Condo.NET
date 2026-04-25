namespace CondoNet.Financial.Core.Entities
{
    public class CondoExpense : BaseEntity
    {
        public string Description { get; set; } = null!;
        public decimal Amount { get; set; }
        public DateTime ExpenseDate { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }

        public Guid FinancialSubSectionId { get; set; }
        public virtual FinancialSubSection FinancialSubSection { get; set; } = null!;

        // Integración Blockchain/Auditoría
        public string DocumentUrl { get; set; } = null!;
        public string DocumentHash { get; set; } = null!; // SHA256 para el Merkle Tree

    }

    public enum ExpenseCategory { Utilities, Maintenance, Payroll, Contingency }
}
