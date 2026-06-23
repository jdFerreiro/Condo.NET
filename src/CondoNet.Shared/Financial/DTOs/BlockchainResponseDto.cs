namespace CondoNet.Shared.Financial.DTOs
{
    public class BlockchainResponseDto
    {
        public bool Success { get; set; }
        public string TransactionHash { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
