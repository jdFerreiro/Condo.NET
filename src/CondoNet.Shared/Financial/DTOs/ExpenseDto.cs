namespace CondoNet.Shared.Financial.DTOs
{
    public class ExpenseDto
    {
        public string ExpenseId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string SubSection { get; set; } = string.Empty;
    }
}
