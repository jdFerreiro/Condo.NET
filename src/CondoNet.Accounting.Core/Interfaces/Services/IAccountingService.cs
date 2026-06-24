using CondoNet.Accounting.Core.Entities;
using CondoNet.Shared;
using CondoNet.Shared.Accounting.DTOs;

namespace CondoNet.Accounting.Core.Interfaces.Services
{
    public interface IAccountingService
    {
        // Catálogo de Cuentas (CRUD)
        Task<Result<Guid>> CreateAccountAsync(CreateAccountRequest request);
        Task<Result<List<AccountResponse>>> GetAccountsByCondoAsync();
        Task<Result<bool>> UpdateAccountAsync(Guid accountId, UpdateAccountRequest request);
        Task<Result<bool>> ToggleAccountStatusAsync(Guid accountId, bool isActive);

        // Libro Diario - Partida Doble
        Task<Result<Guid>> CreateTransactionAsync(CreateTransactionRequest request);
        Task<Result<TransactionDetailResponse>> GetTransactionByIdAsync(Guid transactionId);
        Task<Result<List<TransactionSummaryResponse>>> GetJournalEntriesAsync(DateTime from, DateTime to);
        Task<Result<bool>> VoidTransactionAsync(Guid transactionId);

        // Autómata Contable Avanzado
        Task<Result<Guid>> ProcessAutomatedEntryAsync(ProcessAutomatedEntryRequest request);

        // Mantenimiento de Árbol
        Task<Result<Guid>> AddAccountToTreeAsync(CreateAccountRequest request);
        Task<Result<List<AccountNodeDto>>> GetAccountTreeAsync();

        // Reportes Financieros Automatizados
        Task<Result<BalanceSheetResponse>> GetBalanceSheetAsync();
        Task<Result<IncomeStatementResponse>> GetIncomeStatementAsync(DateTime from, DateTime to);

        // Helpers
        decimal CalculateBalanceImpactLocal(Account.AccountType type, decimal debit, decimal credit);
    }
}
