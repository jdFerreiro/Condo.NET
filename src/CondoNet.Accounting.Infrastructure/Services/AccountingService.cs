using CondoNet.Accounting.Core.Entities;
using CondoNet.Accounting.Core.Interfaces.Services;
using CondoNet.Shared;
using CondoNet.Shared.Accounting.DTOs;
using CondoNet.Shared.Accounting.Events;
using CondoNet.Shared.Interfaces;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Accounting.Infrastructure.Services
{
    public partial class AccountingService(DbContext context,
        IPublishEndpoint publishEndpoint,
        ITenantService tenantService,
        IHttpContextAccessor httpContextAccessor) : IAccountingService
    {
        // 1. CREAR CUENTA (Homologado con tu base)
        public async Task<Result<Guid>> CreateAccountAsync(CreateAccountRequest request)
        {
            var organizationId = tenantService.GetOrganizationId();
            var condominiumId = tenantService.GetCondominiumId();

            var codeExists = await context.Set<Account>()
                .AnyAsync(a => a.CondominiumId == condominiumId && a.Code == request.Code);

            if (codeExists)
                return Result<Guid>.Failure("El código de cuenta ya se encuentra registrado.");

            if (request.ParentAccountId.HasValue)
            {
                var parent = await context.Set<Account>().FindAsync(request.ParentAccountId.Value);
                if (parent == null || parent.CondominiumId != condominiumId)
                    return Result<Guid>.Failure("La cuenta padre especificada no existe.");

                if (parent.IsTransactional)
                    return Result<Guid>.Failure("Una cuenta transaccional no puede ser padre de otra cuenta.");
            }

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

        // 2. OBTENER CUENTAS POR CONDOMINIO
        public async Task<Result<List<AccountResponse>>> GetAccountsByCondoAsync()
        {
            var condominiumId = tenantService.GetCondominiumId();

            var accounts = await context.Set<Account>()
                .Where(a => a.CondominiumId == condominiumId)
                .OrderBy(a => a.Code)
                .Select(a => new AccountResponse(
                    a.Id,
                    a.Code,
                    a.Name,
                    a.Type.ToString(),
                    a.IsTransactional,
                    a.CurrentBalance,
                    a.IsActive))
                .ToListAsync();

            return Result<List<AccountResponse>>.Success(accounts);
        }

        // 3. ACTUALIZAR DETALLES DE CUENTA
        public async Task<Result<bool>> UpdateAccountAsync(Guid accountId, UpdateAccountRequest request)
        {
            var condominiumId = tenantService.GetCondominiumId();

            var account = await context.Set<Account>()
                .FirstOrDefaultAsync(a => a.Id == accountId && a.CondominiumId == condominiumId);

            if (account == null)
                return Result<bool>.Failure("Cuenta no encontrada.");

            if (request.ParentAccountId.HasValue && request.ParentAccountId == accountId)
                return Result<bool>.Failure("Una cuenta no puede ser padre de sí misma.");

            // Si se cambia la cuenta padre, validar que la nueva cuenta padre cumpla las reglas contables
            if (request.ParentAccountId.HasValue && request.ParentAccountId != account.ParentAccountId)
            {
                var parent = await context.Set<Account>().FindAsync(request.ParentAccountId.Value);
                if (parent == null || parent.CondominiumId != condominiumId)
                    return Result<bool>.Failure("La cuenta padre especificada no existe.");

                if (parent.IsTransactional)
                    return Result<bool>.Failure("Una cuenta transaccional no puede ser padre de otra cuenta.");
            }

            account.Name = request.Name;
            account.ParentAccountId = request.ParentAccountId;

            await context.SaveChangesAsync();
            return Result<bool>.Success(true);
        }

        // 4. CAMBIAR ESTADO (Homologado con tu base)
        public async Task<Result<bool>> ToggleAccountStatusAsync(Guid accountId, bool isActive)
        {
            var condominiumId = tenantService.GetCondominiumId();

            var account = await context.Set<Account>()
                .FirstOrDefaultAsync(a => a.Id == accountId && a.CondominiumId == condominiumId);

            if (account == null)
                return Result<bool>.Failure("Cuenta no encontrada.");

            if (!isActive && account.CurrentBalance != 0)
                return Result<bool>.Failure("No se puede desactivar una cuenta con saldo distinto de cero.");

            account.IsActive = isActive;
            await context.SaveChangesAsync();

            var integrationEvent = new AccountStatusToggledEvent(
                AccountId: account.Id,
                IsActive: account.IsActive,
                OccurredOn: DateTime.UtcNow
            );

            await publishEndpoint.Publish(integrationEvent, ctx => StampCorrelationId(ctx));

            return Result<bool>.Success(true);
        }

        // 5. MÉTODO AUXILIAR PARA CORRELATION ID
        private void StampCorrelationId(PublishContext context)
        {
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext != null && httpContext.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId))
            {
                context.CorrelationId = Guid.TryParse(correlationId.ToString(), out var parsedId) ? parsedId : Guid.NewGuid();
            }
            else
            {
                context.CorrelationId = Guid.NewGuid();
            }
        }
    }
}
