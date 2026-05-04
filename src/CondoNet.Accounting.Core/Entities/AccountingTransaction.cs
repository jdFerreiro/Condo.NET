namespace CondoNet.Accounting.Core.Entities;

public class AccountingTransaction
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid CondominiumId { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = null!;
    public string Reference { get; set; } = null!;
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navegación
    public ICollection<AccountingEntry> Entries { get; set; } = new List<AccountingEntry>();
}
