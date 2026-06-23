namespace CondoNet.Shared.Financial.DTOs;

public class ProratedExpenseDto
{
    public string ExpenseId { get; set; } = string.Empty;
    public string SubSectionId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public List<ProratedExpenseUnitDto> Units { get; set; } = new();
}

public class ProratedExpenseUnitDto
{
    public string UnitAccountId { get; set; } = string.Empty;
    public decimal ParticipationPercentage { get; set; }
    public decimal ProratedAmount { get; set; }
}
