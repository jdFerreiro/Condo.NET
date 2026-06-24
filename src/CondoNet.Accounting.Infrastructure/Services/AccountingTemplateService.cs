using CondoNet.Accounting.Core.Entities;
using CondoNet.Accounting.Core.Interfaces;
using CondoNet.Shared;
using CondoNet.Shared.Accounting.DTOs;
using CondoNet.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Accounting.Infrastructure.Services
{
    public class AccountingTemplateService(DbContext context, ITenantService tenantService) : IAccountingTemplateService
    {
        // 1. CREATE: REGISTRAR UNA NUEVA PLANTILLA CON VALIDACIÓN DE EQUILIBRIO PORCENTUAL
        public async Task<Result<Guid>> CreateTemplateAsync(CreateTemplateRequest request)
        {
            var organizationId = tenantService.GetOrganizationId();
            var condominiumId = tenantService.GetCondominiumId();
            var businessEvent = (BusinessEventType)request.EventType;

            // Validar que no exista ya una plantilla para ese mismo tipo de evento de negocio en el condominio
            var templateExists = await context.Set<AccountingTemplate>()
                .AnyAsync(t => t.CondominiumId == condominiumId && t.EventType == businessEvent);

            if (templateExists)
                return Result<Guid>.Failure($"Ya existe una plantilla contable configurada para el evento de negocio: {businessEvent}.");

            if (request.Rules == null || request.Rules.Count == 0)
                return Result<Guid>.Failure("La plantilla debe contener al menos una regla de distribución contable.");

            // Validar consistencia matemática (Suma de Factores del Debe == Suma de Factores del Haber)
            decimal totalDebitFactor = request.Rules.Where(r => r.Movement == (int)RuleMovementType.Debit).Sum(r => r.PercentageFactor);
            decimal totalCreditFactor = request.Rules.Where(r => r.Movement == (int)RuleMovementType.Credit).Sum(r => r.PercentageFactor);

            if (totalDebitFactor != totalCreditFactor)
                return Result<Guid>.Failure($"No se puede guardar la plantilla. Los factores del Debe ({totalDebitFactor}) y Haber ({totalCreditFactor}) deben ser exactamente iguales.");

            var templateId = Guid.NewGuid();
            var template = new AccountingTemplate
            {
                Id = templateId,
                OrganizationId = organizationId,
                CondominiumId = condominiumId,
                EventType = businessEvent,
                Name = request.Name,
                Description = request.Description,
                Rules = []
            };

            // Mapear quirúrgicamente las reglas de distribución asociadas
            foreach (var ruleDto in request.Rules)
            {
                // Verificar que la cuenta asignada a la regla exista en el condominio y esté activa
                var accountExists = await context.Set<Account>()
                    .AnyAsync(a => a.Id == ruleDto.AccountId && a.CondominiumId == condominiumId && a.IsActive && a.IsTransactional);

                if (!accountExists)
                    return Result<Guid>.Failure($"La cuenta con ID {ruleDto.AccountId} no existe, está inactiva o no es transaccional en este condominio.");

                template.Rules.Add(new TemplateRule
                {
                    Id = Guid.NewGuid(),
                    AccountingTemplateId = templateId,
                    AccountId = ruleDto.AccountId,
                    Movement = (RuleMovementType)ruleDto.Movement,
                    PercentageFactor = ruleDto.PercentageFactor
                });
            }

            context.Set<AccountingTemplate>().Add(template);
            await context.SaveChangesAsync();

            return Result<Guid>.Success(templateId);
        }

        // 2. READ ALL: LISTAR LAS CONFIGURACIONES DEL AUTÓMATA CON SUS DETALLES
        public async Task<Result<List<TemplateResponse>>> GetTemplatesByCondoAsync()
        {
            var condominiumId = tenantService.GetCondominiumId();

            var templates = await context.Set<AccountingTemplate>()
                .Include(t => t.Rules)
                .Where(t => t.CondominiumId == condominiumId)
                .OrderBy(t => t.EventType)
                .ToListAsync();

            // Mapeo hacia el DTO estructurado incluyendo los metadatos de las cuentas contables vinculadas
            var response = new List<TemplateResponse>();

            foreach (var t in templates)
            {
                var ruleDtos = new List<TemplateRuleDto>();
                foreach (var r in t.Rules)
                {
                    // Obtener descriptivos rápidos de la cuenta para que el Frontend los pinte con elegancia
                    var account = await context.Set<Account>().FindAsync(r.AccountId);
                    ruleDtos.Add(new TemplateRuleDto(
                        Id: r.Id,
                        AccountId: r.AccountId,
                        AccountName: account?.Name ?? "Cuenta Desconocida",
                        AccountCode: account?.Code ?? "0.0.00.000",
                        Movement: (int)r.Movement,
                        PercentageFactor: r.PercentageFactor
                    ));
                }

                response.Add(new TemplateResponse(
                    Id: t.Id,
                    EventType: (int)t.EventType,
                    Name: t.Name,
                    Description: t.Description,
                    Rules: ruleDtos
                ));
            }

            return Result<List<TemplateResponse>>.Success(response);
        }

        // 3. DELETE: REMOVER PLANTILLA DEL MOTOR AUTOMÁTICO
        public async Task<Result<bool>> DeleteTemplateAsync(Guid templateId)
        {
            var condominiumId = tenantService.GetCondominiumId();

            var template = await context.Set<AccountingTemplate>()
                .Include(t => t.Rules)
                .FirstOrDefaultAsync(t => t.Id == templateId && t.CondominiumId == condominiumId);

            if (template == null)
                return Result<bool>.Failure("Plantilla contable no encontrada.");

            // EF Core se encargará de eliminar las reglas hijas en cascada si está configurado en la base de datos
            context.Set<AccountingTemplate>().Remove(template);
            await context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
    }
}
