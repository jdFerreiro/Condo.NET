using CondoNet.Asset.Core.Entities;
using CondoNet.Asset.Core.Interfaces;
using CondoNet.Asset.Infrastructure.Persistence;
using CondoNet.Shared;
using CondoNet.Shared.Asset.DTOs;
using CondoNet.Shared.Asset.Events;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Asset.Infrastructure.Services;

public class OrganizationService(AssetDbContext context, IPublishEndpoint publishEndpoint, IHttpContextAccessor httpContextAccessor) : IOrganizationService
{
    private readonly AssetDbContext _context = context;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    // 1. CREATE: Registro inicial (Ya implementado previamente)
    public async Task<Result<Guid>> RegisterOrganizationAsync(RegisterOrganizationRequest request)
    {
        var isDuplicate = await _context.Organizations.AnyAsync(o => o.TaxId.Trim() == request.TaxId.Trim());
        if (isDuplicate) return Result<Guid>.Failure("La identificación fiscal ya está registrada.");

        var newOrg = new Organization
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            TaxId = request.TaxId.Trim(),
            BaseCurrency = request.BaseCurrency.Trim().ToUpper(),
            ContactEmail = request.ContactEmail.Trim().ToLower(),
            PhoneNumber = request.PhoneNumber?.Trim(),
            Plan = request.Plan,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Organizations.Add(newOrg);
        await _context.SaveChangesAsync();

        await _publishEndpoint.Publish(
            new OrganizationRegisteredEvent(newOrg.Id, newOrg.Name, newOrg.ContactEmail, newOrg.BaseCurrency), ctx => StampCorrelationId(ctx));
        return Result<Guid>.Success(newOrg.Id);
    }

    // 2. READ: Obtener perfil por ID para paneles de administración
    public async Task<Result<OrganizationResponse>> GetOrganizationByIdAsync(Guid orgId)
    {
        var org = await _context.Organizations
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == orgId);

        if (org == null) return Result<OrganizationResponse>.Failure("Organización no encontrada.");

        return Result<OrganizationResponse>.Success(new OrganizationResponse(
            org.Id, org.Name, org.TaxId, org.ContactEmail, org.PhoneNumber, org.Plan.ToString(), org.IsActive));
    }

    // 3. UPDATE: Actualizar datos de contacto comerciales
    public async Task<Result<bool>> UpdateOrganizationAsync(Guid orgId, UpdateOrgRequest request)
    {
        var org = await _context.Organizations.FirstOrDefaultAsync(o => o.Id == orgId);
        if (org == null) return Result<bool>.Failure("Organización no encontrada.");

        org.Name = request.Name.Trim();
        org.ContactEmail = request.ContactEmail.Trim().ToLower();
        org.PhoneNumber = request.PhoneNumber?.Trim();
        org.LogoUrl = request.LogoUrl?.Trim();

        await _context.SaveChangesAsync();

        // Notificamos el cambio al ecosistema para que Auth.Api y Finanzas actualicen los perfiles visuales
        await _publishEndpoint.Publish(new OrganizationUpdatedEvent(org.Id, org.Name, org.ContactEmail, org.BaseCurrency), ctx => StampCorrelationId(ctx));
        return Result<bool>.Success(true);
    }

    // 4. UPDATE (SaaS): Cambiar el plan de suscripción de la administradora
    public async Task<Result<bool>> UpdateSubscriptionPlanAsync(Guid orgId, int newPlanValue)
    {
        var org = await _context.Organizations.FirstOrDefaultAsync(o => o.Id == orgId);
        if (org == null) return Result<bool>.Failure("Organización no encontrada.");

        org.Plan = (Shared.Asset.SubscriptionPlan)newPlanValue; // Casting seguro del entero al enum
        await _context.SaveChangesAsync();

        return Result<bool>.Success(true);
    }

    // 5. DELETE (Lógico): Suspender o reactivar las operaciones de la empresa
    public async Task<Result<bool>> ToggleOrganizationStatusAsync(Guid orgId, bool isActive)
    {
        var org = await _context.Organizations.FirstOrDefaultAsync(o => o.Id == orgId);
        if (org == null) return Result<bool>.Failure("Organización no encontrada.");

        org.IsActive = isActive;
        await _context.SaveChangesAsync();

        // Notificación crítica: Si se suspende, Auth.Api revocará automáticamente todos los accesos en 'ContextStatus'
        await _publishEndpoint.Publish(new OrganizationStatusChangedEvent(org.Id, org.IsActive, DateTime.UtcNow), ctx => StampCorrelationId(ctx));
        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Método auxiliar privado para inyectar el CorrelationId en el sobre de MassTransit
    /// </summary>
    private void StampCorrelationId(PublishContext context)
    {
        var httpContext = _httpContextAccessor.HttpContext;
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
