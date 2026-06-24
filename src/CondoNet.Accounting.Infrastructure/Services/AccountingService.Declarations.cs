using CondoNet.Accounting.Core.Entities;
using CondoNet.Shared;
using CondoNet.Shared.Accounting.DTOs;
using CondoNet.Shared.Accounting.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Accounting.Infrastructure.Services
{
    public partial class AccountingService
    {
        // VALIDACIÓN Y ADICIÓN DE CUENTAS AL ÁRBOL CONTABLE
        public async Task<Result<Guid>> AddAccountToTreeAsync(CreateAccountRequest request)
        {
            var organizationId = tenantService.GetOrganizationId();
            var condominiumId = tenantService.GetCondominiumId();

            // 1. Validar unicidad del código en el condominio
            var codeExists = await context.Set<Account>()
                .AnyAsync(a => a.CondominiumId == condominiumId && a.Code == request.Code);

            if (codeExists)
                return Result<Guid>.Failure($"El código de cuenta '{request.Code}' ya se encuentra registrado en este condominio.");

            // 2. Validar reglas de jerarquía si la cuenta tiene un Padre asignado
            if (request.ParentAccountId.HasValue)
            {
                var parentAccount = await context.Set<Account>()
                    .FirstOrDefaultAsync(a => a.Id == request.ParentAccountId.Value && a.CondominiumId == condominiumId);

                if (parentAccount == null)
                    return Result<Guid>.Failure("La cuenta padre especificada no existe en este condominio.");

                // Regla de Oro 1: Una cuenta transaccional (que recibe asientos) no puede tener hijos
                if (parentAccount.IsTransactional)
                    return Result<Guid>.Failure($"La cuenta padre '{parentAccount.Name}' es de tipo transaccional. Para agregar subcuentas, el padre debe ser una cuenta de agrupación.");

                // Regla de Oro 2: Coherencia de Naturaleza (Un Activo solo puede colgar de un Activo)
                if ((int)parentAccount.Type != request.Type)
                    return Result<Guid>.Failure($"Conflicto de naturaleza en el árbol. No puedes colgar una cuenta de tipo {(Account.AccountType)request.Type} bajo un padre de tipo {parentAccount.Type}.");

                // Regla de Oro 3: Coherencia de Código (El código del hijo debe comenzar con el código del padre)
                if (!request.Code.StartsWith(parentAccount.Code))
                    return Result<Guid>.Failure($"Inconsistencia en la codificación del árbol. El código del hijo '{request.Code}' debe tener como prefijo el código del padre '{parentAccount.Code}'.");
            }
            else
            {
                // Si no tiene padre, se asume que es una cuenta raíz (Ej: "1" para Activos, "2" para Pasivos)
                // Regla: Las cuentas raíz nunca deben ser transaccionales por diseño estándar
                if (request.IsTransactional)
                    return Result<Guid>.Failure("Por buenas prácticas contables, las cuentas raíz del árbol no pueden ser transaccionales; deben ser de agrupación.");
            }

            // 3. Persistir la nueva rama del árbol
            var account = new Account
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                CondominiumId = condominiumId,
                Code = request.Code,
                Name = request.Name,
                Type = (Account.AccountType)request.Type,
                IsTransactional = request.IsTransactional,
                ParentAccountId = request.ParentAccountId,
                CurrentBalance = 0,
                IsActive = true
            };

            context.Set<Account>().Add(account);
            await context.SaveChangesAsync();

            // Despachar evento a RabbitMQ para sincronización (Reutiliza tu bus configurado)
            var integrationEvent = new AccountCreatedEvent(
                AccountId: account.Id,
                CondominiumId: account.CondominiumId,
                Code: account.Code,
                Name: account.Name,
                Type: account.Type.ToString(),
                OccurredOn: DateTime.UtcNow
            );
            await publishEndpoint.Publish(integrationEvent, ctx => StampCorrelationId(ctx));

            return Result<Guid>.Success(account.Id);
        }
    }
}
