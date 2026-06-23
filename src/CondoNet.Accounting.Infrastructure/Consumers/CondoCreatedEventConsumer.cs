using CondoNet.Accounting.Core.Entities;
using CondoNet.Accounting.Core.Interfaces.Repositories;
using CondoNet.Shared.Asset.Events;
using MassTransit;
using static CondoNet.Accounting.Core.Entities.Account;

namespace CondoNet.Accounting.Infrastructure.Consumers;

public class CondoCreatedEventConsumer : IConsumer<CondominiumCreatedEvent>
{
    private readonly IAccountRepository _accountRepository;

    public CondoCreatedEventConsumer(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task Consume(ConsumeContext<CondominiumCreatedEvent> context)
    {
        var evt = context.Message;

        var accounts = new List<Account>
        {
            new() { Id = Guid.NewGuid(), OrganizationId = evt.OrganizationId, CondominiumId = evt.CondominiumId, Code = "1", Name = "Activo", Type = AccountType.Asset, IsActive = true, CurrentBalance = 0, IsTransactional = false, ParentAccountId = null },
            new() { Id = Guid.NewGuid(), OrganizationId = evt.OrganizationId, CondominiumId = evt.CondominiumId, Code = "2", Name = "Pasivo", Type = AccountType.Liability, IsActive = true, CurrentBalance = 0, IsTransactional = false, ParentAccountId = null },
            new() { Id = Guid.NewGuid(), OrganizationId = evt.OrganizationId, CondominiumId = evt.CondominiumId, Code = "3", Name = "Patrimonio", Type = AccountType.Equity, IsActive = true, CurrentBalance = 0, IsTransactional = false, ParentAccountId = null },
            new() { Id = Guid.NewGuid(), OrganizationId = evt.OrganizationId, CondominiumId = evt.CondominiumId, Code = "4", Name = "Ingreso", Type = AccountType.Revenue, IsActive = true, CurrentBalance = 0, IsTransactional = false, ParentAccountId = null },
            new() { Id = Guid.NewGuid(), OrganizationId = evt.OrganizationId, CondominiumId = evt.CondominiumId, Code = "5", Name = "Gasto", Type = AccountType.Expense, IsActive = true, CurrentBalance = 0, IsTransactional = false, ParentAccountId = null }
        };

        foreach (var account in accounts)
            await _accountRepository.AddAsync(account);
    }
}
