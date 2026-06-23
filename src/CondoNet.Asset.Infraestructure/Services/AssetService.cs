using CondoNet.Asset.Core.Entities;
using CondoNet.Asset.Core.Interfaces;
using CondoNet.Asset.Infrastructure.Persistence;
using CondoNet.Shared.Asset.DTOs;
using CondoNet.Shared.Asset.Events;
using CondoNet.Shared.Interfaces;
using MassTransit;
using Microsoft.AspNetCore.Http; // <-- Indispensable para HttpContext
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Asset.Infrastructure.Services;

// CONSTRUCTOR PRIMARIO: Inyectamos IHttpContextAccessor manteniendo limpia la sintaxis sin warnings CS9124
public class AssetService(
    AssetDbContext context,
    IPublishEndpoint publishEndpoint,
    ITenantService tenantService,
    IHttpContextAccessor httpContextAccessor) : IAssetService
{
    public async Task<bool> BulkImportUnitsAsync(BulkImportRequest request)
    {
        var tenantOrgId = tenantService.GetOrganizationId();
        var tenantCondoId = tenantService.GetCondominiumId();

        if (tenantCondoId != request.CondominiumId)
        {
            throw new UnauthorizedAccessException("Operación rechazada: No tiene permisos sobre este condominio.");
        }

        // Regla de Oro: Suma exacta de alícuotas del lote = 100.0000%
        decimal totalAliquot = request.Units.Sum(u => u.Aliquot);
        if (totalAliquot != 100.0000m)
        {
            throw new InvalidOperationException($"La suma de alícuotas es {totalAliquot}%. Debe ser exactamente 100.0000%.");
        }

        var strategy = context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                // Carga masiva de torres existentes para optimización de consultas (Evita el problema N+1)
                var existingTowers = await context.Towers
                    .Where(t => t.CondominiumId == request.CondominiumId)
                    .ToListAsync();

                var uniqueTowerNames = request.Units
                    .Select(u => u.TowerName.Trim().ToUpper())
                    .Distinct()
                    .ToList();

                var finalTowersMap = new List<Tower>(existingTowers);

                var towersToCreate = uniqueTowerNames
                    .Where(name => !existingTowers.Any(et => et.Name.Trim().ToUpper() == name))
                    .Select(name => new Tower { Id = Guid.NewGuid(), Name = name, CondominiumId = request.CondominiumId, OrganizationId = tenantOrgId })
                    .ToList();

                if (towersToCreate.Count > 0)
                {
                    await context.Towers.AddRangeAsync(towersToCreate);
                    await context.SaveChangesAsync();
                    finalTowersMap.AddRange(towersToCreate);
                }

                // Creación de lote de Unidades inmobiliarias mapeando campos obligatorios
                var unitsToInsert = request.Units.Select(dto =>
                {
                    var normalizedName = dto.TowerName.Trim().ToUpper();
                    var towerTarget = finalTowersMap.First(t => t.Name.Trim().ToUpper() == normalizedName);

                    return new Core.Entities.Unit
                    {
                        Id = Guid.NewGuid(),
                        Identifier = dto.Identifier.Trim(),
                        Floor = dto.Floor?.Trim() ?? "1",
                        Aliquot = dto.Aliquot,
                        Type = dto.Type,
                        Alias = dto.Alias?.Trim(),
                        AreaSquareMeters = dto.AreaSquareMeters,
                        LinkedAssets = [],
                        OwnerEmail = dto.OwnerEmail.Trim().ToLower(),
                        TowerId = towerTarget.Id, // REGLA DE RENDIMIENTO: Mapeamos solo el Id para acelerar el guardado masivo
                        OrganizationId = tenantOrgId
                    };
                }).ToList();

                await context.Units.AddRangeAsync(unitsToInsert);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Consistencia Eventual masiva
                var eventUnits = unitsToInsert.Select(u => new ImportedUnitDto(u.Id, u.Identifier, u.Aliquot, u.OwnerEmail)).ToList();

                // REGLA DE TRAZABILIDAD: Despachamos el evento inyectando el CorrelationId de la petición web original
                await publishEndpoint.Publish(new UnitsImported(tenantOrgId, request.CondominiumId, eventUnits), contextEnvelope =>
                {
                    var httpContext = httpContextAccessor.HttpContext;
                    if (httpContext != null && httpContext.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId))
                    {
                        // Estampamos el ID HTTP original en los metadatos de las cabeceras de RabbitMQ
                        contextEnvelope.CorrelationId = Guid.TryParse(correlationId.ToString(), out var parsedId) ? parsedId : Guid.NewGuid();
                    }
                    else
                    {
                        contextEnvelope.CorrelationId = Guid.NewGuid();
                    }
                });

                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }
}
