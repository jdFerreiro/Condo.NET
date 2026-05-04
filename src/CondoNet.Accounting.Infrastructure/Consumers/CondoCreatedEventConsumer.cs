using CondoNet.Shared.Events.Asset;
using CondoNet.Accounting.Core.Entities;
using CondoNet.Accounting.Core.Repositories;
using MassTransit;

namespace CondoNet.Accounting.Infrastructure.Consumers;

public class CondoCreatedEventConsumer : IConsumer<CondoCreatedEvent>
{
    private readonly IAccountRepository _accountRepository;

    public CondoCreatedEventConsumer(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task Consume(ConsumeContext<CondoCreatedEvent> context)
    {
        var evt = context.Message;

        var accounts = new List<Account>
        {
            new() { Id = Guid.NewGuid(), OrganizationId = evt.OrganizationId, CondominiumId = evt.CondominiumId, Code = "1", Name = "Activo", Type = "Activo", IsActive = true, Balance = 0 },
            new() { Id = Guid.NewGuid(), OrganizationId = evt.OrganizationId, CondominiumId = evt.CondominiumId, Code = "2", Name = "Pasivo", Type = "Pasivo", IsActive = true, Balance = 0 },
            new() { Id = Guid.NewGuid(), OrganizationId = evt.OrganizationId, CondominiumId = evt.CondominiumId, Code = "3", Name = "Patrimonio", Type = "Patrimonio", IsActive = true, Balance = 0 },
            new() { Id = Guid.NewGuid(), OrganizationId = evt.OrganizationId, CondominiumId = evt.CondominiumId, Code = "4", Name = "Ingreso", Type = "Ingreso", IsActive = true, Balance = 0 },
            new() { Id = Guid.NewGuid(), OrganizationId = evt.OrganizationId, CondominiumId = evt.CondominiumId, Code = "5", Name = "Gasto", Type = "Gasto", IsActive = true, Balance = 0 }
        };

        foreach (var account in accounts)
            await _accountRepository.AddAsync(account);
    }
}
