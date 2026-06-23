using CondoNet.Shared.Financial.DTOs;

namespace CondoNet.Financial.Core.Services
{
    public interface IBillingService
    {
        Task<List<MonthlyBillDto>> GenerateMonthlyBillAsync(DateTime period, CancellationToken cancellationToken = default);
        Task<ProratedExpenseDto> GenerateExpenseAsync(ExpenseDto expense, CancellationToken cancellationToken = default);
        Task<string> CalculateMerkleRootAsync(DateTime period, CancellationToken cancellationToken = default);
        Task UpdateExpenseAsync(Guid expenseId, ExpenseDto updatedExpense, CancellationToken cancellationToken = default);
    }
}
