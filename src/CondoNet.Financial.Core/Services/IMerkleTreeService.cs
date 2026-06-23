using CondoNet.Shared.Financial.DTOs;

namespace CondoNet.Financial.Core.Services
{
    public interface IMerkleTreeService
    {
        Task<string> GenerateMerkleRootAsync(IEnumerable<ExpenseDto> expenses, CancellationToken cancellationToken = default);
        bool ValidateMerkleRoot(IEnumerable<ExpenseDto> expenses, string merkleRoot);
    }
}
