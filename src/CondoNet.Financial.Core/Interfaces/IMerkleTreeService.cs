using CondoNet.Shared.DTOs.Financial;

namespace CondoNet.Financial.Core.Interfaces
{
    public interface IMerkleTreeService
    {
        Task<string> GenerateMerkleRootAsync(IEnumerable<ExpenseDto> expenses, CancellationToken cancellationToken = default);
        bool ValidateMerkleRoot(IEnumerable<ExpenseDto> expenses, string merkleRoot);
    }
}
