namespace CondoNet.Accounting.Core.Entities;

public class Account
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid CondominiumId { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Type { get; set; } = null!; // Activo, Pasivo, Patrimonio, Ingreso, Gasto
    public bool IsActive { get; set; }
    public decimal Balance { get; set; }

    // Navegación
    public ICollection<AccountingEntry> Entries { get; set; } = new List<AccountingEntry>();
}
