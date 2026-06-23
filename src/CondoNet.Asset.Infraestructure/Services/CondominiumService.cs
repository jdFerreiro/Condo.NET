using CondoNet.Asset.Core.Entities;
using CondoNet.Asset.Core.Interfaces;
using CondoNet.Asset.Infrastructure.Persistence;
using CondoNet.Shared;
using CondoNet.Shared.Asset.DTOs;
using CondoNet.Shared.Asset.Events;
using CondoNet.Shared.Interfaces;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Asset.Infrastructure.Services;

public class CondominiumService(
    AssetDbContext context,
    IPublishEndpoint publishEndpoint,
    ITenantService tenantService,
    IHttpContextAccessor httpContextAccessor) : ICondominiumService
{
    private readonly AssetDbContext _context = context;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;
    private readonly ITenantService _tenantService = tenantService;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    // 1. CREATE: Alta de un Condominio (Conectado a la Organización del Administrador)
    public async Task<Result<Guid>> CreateCondominiumAsync(CreateCondominiumRequest request)
    {
        var tenantOrgId = _tenantService.GetOrganizationId();

        var isDuplicate = await _context.Condominiums.AnyAsync(c => c.TaxId.Trim() == request.TaxId.Trim());
        if (isDuplicate)
            return Result<Guid>.Failure("La identificación fiscal de este condominio ya se encuentra dada de alta.");

        var newCondo = new Condominium
        {
            Id = Guid.NewGuid(),
            OrganizationId = tenantOrgId,
            Name = request.Name.Trim(),
            TaxId = request.TaxId.Trim(),
            Address = request.Address.Trim(),
            ReserveFundPercentage = request.ReserveFundPercentage,
            Units = []
        };

        _context.Condominiums.Add(newCondo);
        await _context.SaveChangesAsync();

        // Notificación de Consistencia Eventual para Auth, Finanzas y Contabilidad
        await _publishEndpoint.Publish(new CondominiumCreatedEvent(
            newCondo.Id,
            newCondo.OrganizationId,
            newCondo.Name,
            newCondo.ReserveFundPercentage
        ), ctx => StampCorrelationId(ctx));

        return Result<Guid>.Success(newCondo.Id);
    }

    // 2. READ (Individual): Obtener detalle de un condominio validando aislamiento Multi-Tenant
    public async Task<Result<GetCondominiumResponse>> GetCondominiumByIdAsync(Guid condoId)
    {
        var tenantOrgId = _tenantService.GetOrganizationId();

        // El filtro por OrganizationId asegura que una administradora no lea condominios ajenos
        var condo = await _context.Condominiums
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == condoId && c.OrganizationId == tenantOrgId);

        if (condo == null)
            return Result<GetCondominiumResponse>.Failure("El condominio especificado no existe o no pertenece a su organización.");

        return Result<GetCondominiumResponse>.Success(new GetCondominiumResponse(
            condo.Id, condo.OrganizationId, condo.Name, condo.TaxId, condo.Address, condo.ReserveFundPercentage));
    }

    // 3. READ (Masivo): Listar todos los condominios que administra la organización activa
    public async Task<Result<List<GetCondominiumResponse>>> GetCondominiumsByOrganizationAsync()
    {
        var tenantOrgId = _tenantService.GetOrganizationId();

        var condos = await _context.Condominiums
            .AsNoTracking()
            .Where(c => c.OrganizationId == tenantOrgId)
            .Select(c => new GetCondominiumResponse
            (
                c.Id,
                c.OrganizationId,
                c.Name,
                c.TaxId,
                c.Address,
                c.ReserveFundPercentage
            )).ToListAsync();
        return Result<List<GetCondominiumResponse>>.Success(condos);
    }

    // 4. UPDATE: Actualizar datos de infraestructura o porcentaje de fondo de reserva
    public async Task<Result<bool>> UpdateCondominiumAsync(Guid condoId, UpdateCondominiumRequest request)
    {
        var tenantOrgId = _tenantService.GetOrganizationId();

        var condo = await _context.Condominiums
            .FirstOrDefaultAsync(c => c.Id == condoId && c.OrganizationId == tenantOrgId);

        if (condo == null)
            return Result<bool>.Failure("Condominio no encontrado en su entorno de organización.");

        condo.Name = request.Name.Trim();
        condo.Address = request.Address.Trim();
        condo.ReserveFundPercentage = request.ReserveFundPercentage;

        await _context.SaveChangesAsync();

        // REGLA DE NEGOCIO CRÍTICA: Notificar el cambio a Finanzas/Contabilidad.
        // Si el 'ReserveFundPercentage' muta, los algoritmos de cálculo de facturas y cuotas extras 
        // de 'Financial Service' deben recalcular sus variables de inmediato para este condominio.
        await _publishEndpoint.Publish(new CondominiumUpdatedEvent(
            condo.Id,
            condo.OrganizationId,
            condo.Name,
            condo.ReserveFundPercentage
        ), ctx => StampCorrelationId(ctx));

        return Result<bool>.Success(true);
    }

    // 5. DELETE (Lógico): Eliminar/Bajar un condominio de la administración activa
    public async Task<Result<bool>> DeleteCondominiumAsync(Guid condoId)
    {
        var tenantOrgId = _tenantService.GetOrganizationId();

        var condo = await _context.Condominiums
            .Include(c => c.Units) // Evaluamos si tiene unidades amarradas
            .FirstOrDefaultAsync(c => c.Id == condoId && c.OrganizationId == tenantOrgId);

        if (condo == null)
            return Result<bool>.Failure("Condominio no encontrado.");

        // Regla de Negocio de Integridad Física: No puedes borrar un condominio que aún tenga departamentos cargados
        if (condo.Units.Count > 0)
            return Result<bool>.Failure("No es posible dar de baja un condominio que aún contiene unidades registradas. Realice una migración de activos primero.");

        _context.Condominiums.Remove(condo);
        await _context.SaveChangesAsync();

        // Notificación de remoción para que Auth.API purgue los contextos de usuarios obsoletos de este edificio
        await _publishEndpoint.Publish(new CondominiumDeletedEvent(condo.Id, tenantOrgId), ctx => StampCorrelationId(ctx));

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
