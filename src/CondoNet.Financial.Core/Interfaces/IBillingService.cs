using CondoNet.Shared.DTOs.Financial;

namespace CondoNet.Financial.Core.Interfaces
{
    public interface IBillingService
    {
        Task GenerateMonthlyBillAsync(DateTime period, CancellationToken cancellationToken = default);
        Task GenerateExpenseAsync(ExpenseDto expense, CancellationToken cancellationToken = default);
        Task<string> CalculateMerkleRootAsync(DateTime period, CancellationToken cancellationToken = default);
    }
}
