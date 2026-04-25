namespace CondoNet.Financial.Core.Entities
{
    public class CondoExpense : BaseEntity
    {
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public ExpenseCategory Category { get; set; }
        public string DocumentUrl { get; set; } = string.Empty; // Link a S3 con la factura PDF
        public string DocumentHash { get; set; } = string.Empty; // SHA256 del archivo físico

        public int Month { get; set; }
        public int Year { get; set; }

        public virtual Guid? ApprovedById { get; set; }
    }

    public enum ExpenseCategory { Utilities, Maintenance, Payroll, Contingency }
}
