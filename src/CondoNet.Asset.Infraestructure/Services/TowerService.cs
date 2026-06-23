using CondoNet.Asset.Core.Entities;
using CondoNet.Asset.Core.Interfaces;
using CondoNet.Asset.Infrastructure.Persistence;
using CondoNet.Shared;
using CondoNet.Shared.Asset;
using CondoNet.Shared.Asset.DTOs;
using CondoNet.Shared.Asset.Events; // <-- Importación para los nuevos eventos
using CondoNet.Shared.Interfaces;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Asset.Infrastructure.Services;

// CONSTRUCTOR PRIMARIO: Inyección directa sin duplicar estados de campos privados en memoria
public class TowerService(
    AssetDbContext context,
    ITenantService tenantService,
    IPublishEndpoint publishEndpoint,
    IHttpContextAccessor httpContextAccessor) : ITowerService
{
    // 1. CREATE: Registrar una torre individual en el condominio activo
    public async Task<Result<Guid>> CreateTowerAsync(string name)
    {
        var tenantOrgId = tenantService.GetOrganizationId();
        var tenantCondoId = tenantService.GetCondominiumId();

        // Regla de negocio: Evitar nombres de torres duplicados en el mismo condominio
        var isDuplicate = await context.Towers.AnyAsync(t =>
            t.Name.Trim().ToUpper() == name.Trim().ToUpper() && t.CondominiumId == tenantCondoId);

        if (isDuplicate)
            return Result<Guid>.Failure($"La torre o bloque '{name}' ya se encuentra registrada en este condominio.");

        var newTower = new Tower
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            CondominiumId = tenantCondoId,
            OrganizationId = tenantOrgId,
            Units = []
        };

        context.Towers.Add(newTower);
        await context.SaveChangesAsync();

        // REGLA DE NEGOCIO ASÍNCRONA: Notificamos la existencia de la torre a la contabilidad
        await publishEndpoint.Publish(new TowerCreatedEvent(
            newTower.Id, newTower.Name, tenantCondoId, tenantOrgId),
            ctx => StampCorrelationId(ctx));

        return Result<Guid>.Success(newTower.Id);
    }

    // 2. READ: Listar torres pertenecientes al condominio de la sesión activa
    public async Task<List<TowerResponse>> GetTowersByCondoAsync()
    {
        var tenantCondoId = tenantService.GetCondominiumId();

        return await context.Towers
            .AsNoTracking()
            .Where(t => t.CondominiumId == tenantCondoId)
            .Select(t => new TowerResponse
            {
                Id = t.Id,
                Name = t.Name,
                TotalUnits = t.Units.Count,
                TotalHabitationalUnits = t.Units.Count(u => u.Type == UnitType.Habitational),
                TotalCommercialUnits = t.Units.Count(u => u.Type == UnitType.Commercial)
            })
            .ToListAsync();
    }

    // 3. UPDATE: Renombrar un bloque o torre
    public async Task<Result<bool>> UpdateTowerAsync(Guid towerId, string newName)
    {
        var tenantCondoId = tenantService.GetCondominiumId();

        var tower = await context.Towers.FirstOrDefaultAsync(t => t.Id == towerId && t.CondominiumId == tenantCondoId);
        if (tower == null) return Result<bool>.Failure("Torre no encontrada.");

        tower.Name = newName.Trim();
        await context.SaveChangesAsync();

        return Result<bool>.Success(true);
    }

    // 4. DELETE: Remover una torre validando integridad física de negocio
    public async Task<Result<bool>> DeleteTowerAsync(Guid towerId)
    {
        var tenantCondoId = tenantService.GetCondominiumId();
        var tenantOrgId = tenantService.GetOrganizationId();

        var tower = await context.Towers
            .Include(t => t.Units)
            .FirstOrDefaultAsync(t => t.Id == towerId && t.CondominiumId == tenantCondoId);

        if (tower == null) return Result<bool>.Failure("Torre no encontrada.");

        // REGLA DE ORO DE INTEGRIDAD: No puedes demoler lógicamente una torre que tenga departamentos registrados
        if (tower.Units.Count > 0)
            return Result<bool>.Failure("Operación denegada: La torre contiene unidades o apartamentos asociados. Muévalos o elimínelos primero.");

        context.Towers.Remove(tower);
        await context.SaveChangesAsync();

        // REGLA DE NEGOCIO ASÍNCRONA: Notificamos la purga del nodo al ecosistema distribuido
        await publishEndpoint.Publish(new TowerDeletedEvent(
            towerId, tenantCondoId, tenantOrgId),
            ctx => StampCorrelationId(ctx));

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Método auxiliar privado para inyectar el CorrelationId en el sobre de MassTransit
    /// </summary>
    private void StampCorrelationId(PublishContext contextEnvelope)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext != null && httpContext.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId))
        {
            contextEnvelope.CorrelationId = Guid.TryParse(correlationId.ToString(), out var parsedId) ? parsedId : Guid.NewGuid();
        }
        else
        {
            contextEnvelope.CorrelationId = Guid.NewGuid();
        }
    }
}
