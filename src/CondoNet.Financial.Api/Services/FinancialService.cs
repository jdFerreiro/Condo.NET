using CondoNet.Financial.Core.Dtos;
using CondoNet.Financial.Core.Entities;
using CondoNet.Financial.Core.Interfaces;

namespace CondoNet.Financial.Api.Services
{
    public class FinancialService(IFinancialRepository repository) : IFinancialService
    {
        private readonly IFinancialRepository _repository = repository;

        public async Task<bool> ExecutePaymentAsync(int accountId, decimal amount)
        {
            var balance = await _repository.GetBalanceAsync(accountId);
            if (balance < amount) return false;

            // Lógica para registrar el pago
            return true;
        }

        public async Task<TransactionResultDto> RegisterTransactionAsync(CreateTransactionDto dto)
        {
            var transaction = new Transaction
            {
                AccountId = dto.AccountId,
                Amount = dto.Amount,
                Description = dto.Description,
                TransactionDate = DateTime.UtcNow,
                Type = (TransactionType)dto.Type
            };

            await _repository.AddTransactionAsync(transaction);
            return new TransactionResultDto(true, "Transacción registrada exitosamente.");
        }

        public async Task<decimal> GetCurrentBalanceAsync(int accountId)
            => await _repository.GetBalanceAsync(accountId);
    }
}
