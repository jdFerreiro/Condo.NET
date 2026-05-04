namespace CondoNet.Accounting.Core.DTOs;

public class AccountDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Type { get; set; } = null!;
    public bool IsActive { get; set; }
    public decimal Balance { get; set; }
    // OrganizationId y CondominiumId se asignan en backend
}
