using CondoNet.Shared.DTOs.Financial;

namespace CondoNet.Financial.Core.Services
{
    public interface IMerkleTreeService
    {
        Task<string> GenerateMerkleRootAsync(IEnumerable<ExpenseDto> expenses, CancellationToken cancellationToken = default);
        bool ValidateMerkleRoot(IEnumerable<ExpenseDto> expenses, string merkleRoot);
    }
}
