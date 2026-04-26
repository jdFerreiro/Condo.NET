namespace CondoNet.Shared.DTOs.Financial
{
    public class AccountStatusDto
    {
        public string UnitId { get; set; } = string.Empty;
        public decimal CurrentDebt { get; set; }
        public decimal CreditBalance { get; set; }
        public DateTime LastUpdate { get; set; }
        public string LastExpenseMerkleRoot { get; set; } = string.Empty;
    }
}
