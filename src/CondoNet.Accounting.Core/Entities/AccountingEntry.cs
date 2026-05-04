namespace CondoNet.Accounting.Core.Entities;

public class AccountingEntry
{
    public Guid Id { get; set; }
    public Guid TransactionId { get; set; }
    public Guid AccountId { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string Description { get; set; } = null!;

    // Navegación
    public AccountingTransaction Transaction { get; set; } = null!;
    public Account Account { get; set; } = null!;
}
