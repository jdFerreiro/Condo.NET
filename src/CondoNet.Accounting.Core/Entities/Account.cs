namespace CondoNet.Accounting.Core.Entities
{
    public class Account
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid CondominiumId { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public AccountType Type { get; set; }
        public bool IsTransactional { get; set; } // True si recibe asientos, False si es cuenta de agrupación (Padre)
        public Guid? ParentAccountId { get; set; }
        public decimal CurrentBalance { get; set; }
        public bool IsActive { get; set; } = true;

        public enum AccountType { Asset = 1, Liability = 2, Equity = 3, Revenue = 4, Expense = 5 }
    }
}
