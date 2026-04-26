namespace CondoNet.Shared.DTOs.Financial
{
    public class PaymentValidationResult
    {
        public string Status { get; set; } = string.Empty;
        public bool AdjustmentRequired { get; set; }
        public decimal FundImpact { get; set; }
        public decimal RemainingDebtUSD { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
