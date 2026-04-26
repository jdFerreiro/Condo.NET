namespace CondoNet.Shared.DTOs.Financial
{
    public class BlockchainResponseDto
    {
        public bool Success { get; set; }
        public string TransactionHash { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
    }
}
