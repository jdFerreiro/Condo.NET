using CondoNet.Financial.Core.Dtos;

namespace CondoNet.Financial.Core.Interfaces
{
    public interface IFinancialService
    {
        Task<bool> ExecutePaymentAsync(int accountId, decimal amount);
        // Otros métodos de lógica de negocio...
        Task<IEnumerable<TransactionDto>> GetTransactionsByAccountAsync(int accountId);
        Task<TransactionResultDto> RegisterTransactionAsync(CreateTransactionDto transaction);

        // Gestión de Saldos
        Task<decimal> GetCurrentBalanceAsync(int accountId);

        // Gestión de Facturación (Lo que antes era Billing)
        Task<InvoiceDto> GenerateMonthlyInvoiceAsync(int accountId, decimal amount, string concept);
        Task<bool> MarkInvoiceAsPaidAsync(int invoiceId);
    }
}
