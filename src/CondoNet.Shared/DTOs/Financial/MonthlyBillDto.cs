namespace CondoNet.Shared.DTOs.Financial;

public class MonthlyBillDto
{
    public string UnitAccountId { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public List<MonthlyBillItemDto> Items { get; set; } = new();
}

public class MonthlyBillItemDto
{
    public string ExpenseId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string SubSectionId { get; set; } = string.Empty;
}
