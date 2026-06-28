namespace CondoNet.Accounting.Core.DTOs;

public class AccountingTransactionDto
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = null!;
    public string Reference { get; set; } = null!;
    public List<AccountingEntryDto> Entries { get; set; } = new();
}

public class AccountingEntryDto
{
    public Guid Id { get; set; }
    public string AccountCode { get; set; } = null!;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string Description { get; set; } = null!;
}
