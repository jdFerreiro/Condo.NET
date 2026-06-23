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

public class UnitService(
    AssetDbContext context,
    IPublishEndpoint publishEndpoint,
    ITenantService tenantService,
    IHttpContextAccessor httpContextAccessor) : IUnitService
{

    // 1. CREATE: Creación e inserción individual de un inmueble (Ya implementado)
    public async Task<Result<Guid>> CreateUnitAsync(CreateUnitRequest request)
    {
        var tenantOrgId = tenantService.GetOrganizationId();

        var towerExists = await context.Towers.AnyAsync(t => t.Id == request.TowerId);
        if (!towerExists)
            return Result<Guid>.Failure("La torre o bloque especificado no existe.");

        var isDuplicate = await context.Units.AnyAsync(u =>
            u.Identifier.Trim().ToUpper() == request.Identifier.Trim().ToUpper() && u.TowerId == request.TowerId);
        if (isDuplicate)
            return Result<Guid>.Failure($"La unidad '{request.Identifier}' ya está registrada en esta torre.");

        var newUnit = new Core.Entities.Unit
        {
            Id = Guid.NewGuid(),
            Identifier = request.Identifier.Trim(),
            Floor = request.Floor.Trim(),
            AreaSquareMeters = request.AreaSquareMeters,
            Alias = request.Alias?.Trim(),
            Aliquot = request.Aliquot,
            Type = request.Type,
            OwnerEmail = request.OwnerEmail.Trim().ToLower(),
            TowerId = request.TowerId,
            OrganizationId = tenantOrgId
        };

        context.Units.Add(newUnit);
        await context.SaveChangesAsync();

        // Notificación de consistencia para abrir saldos financieros downstream
        await publishEndpoint.Publish(
            new IndividualUnitCreatedEvent(newUnit.Id, newUnit.Identifier, newUnit.Aliquot, newUnit.OwnerEmail, tenantOrgId), ctx => StampCorrelationId(ctx));

        return Result<Guid>.Success(newUnit.Id);
    }

    // 2. READ: Obtener unidades por propietario (Ya implementado)
    public async Task<List<UnitResponse>> GetUnitsByOwnerAsync(string ownerId)
    {
        return await context.Units
            .AsNoTracking()
            .Where(u => u.OwnerEmail == ownerId)
            .Select(u => new UnitResponse
            {
                Id = u.Id,
                TowerId = u.TowerId,
                Identifier = u.Identifier,
                Floor = u.Floor,
                AreaSquareMeters = u.AreaSquareMeters,
                Alias = u.Alias,
                Aliquot = u.Aliquot,
                OwnerEmail = u.OwnerEmail,
                Type = u.Type.ToString()
            })
            .ToListAsync();
    }

    // 3. READ: Listar todas las unidades de una Torre específica (Multi-Tenant protegido)
    public async Task<Result<List<UnitResponse>>> GetUnitsByTowerAsync(Guid towerId)
    {
        var tenantOrgId = tenantService.GetOrganizationId();

        var units = await context.Units
            .AsNoTracking()
            .Where(u => u.TowerId == towerId && u.OrganizationId == tenantOrgId)
            .Select(u => new UnitResponse
            {
                Id = u.Id,
                TowerId = u.TowerId,
                Identifier = u.Identifier,
                Floor = u.Floor,
                AreaSquareMeters = u.AreaSquareMeters,
                Alias = u.Alias,
                Aliquot = u.Aliquot,
                OwnerEmail = u.OwnerEmail,
                Type = u.Type.ToString()
            })
            .ToListAsync();

        return Result<List<UnitResponse>>.Success(units);
    }

    // 4. UPDATE: Modificar datos de una unidad e informar a Finanzas si cambia alícuota o dueño
    public async Task<Result<bool>> UpdateUnitAsync(Guid unitId, UpdateUnitRequest request)
    {
        var tenantOrgId = tenantService.GetOrganizationId();

        var unit = await context.Units.FirstOrDefaultAsync(u => u.Id == unitId && u.OrganizationId == tenantOrgId);
        if (unit == null) return Result<bool>.Failure("La unidad especificada no existe en su organización.");

        // Guardamos banderas para evaluar si se requiere emitir alertas contables downstream
        bool ownerChanged = unit.OwnerEmail.Trim().ToLower() != request.OwnerEmail.Trim().ToLower();
        bool aliquotChanged = unit.Aliquot != request.Aliquot;

        unit.Identifier = request.Identifier.Trim();
        unit.Floor = request.Floor.Trim();
        unit.AreaSquareMeters = request.AreaSquareMeters;
        unit.Alias = request.Alias?.Trim();
        unit.Aliquot = request.Aliquot;
        unit.Type = request.Type;
        unit.OwnerEmail = request.OwnerEmail.Trim().ToLower();

        await context.SaveChangesAsync();

        // REGLA DE NEGOCIO: Si muta el dueño o la alícuota de participación, notificamos asíncronamente
        // para que Financial Service ajuste los cálculos de cuentas por cobrar de forma inmediata.
        if (ownerChanged || aliquotChanged)
        {
            await publishEndpoint.Publish(new UnitMetadataUpdatedEvent(
                unit.Id,
                unit.OrganizationId,
                unit.Identifier,
                unit.OwnerEmail,
                unit.Aliquot
            ), ctx => StampCorrelationId(ctx));
        }

        return Result<bool>.Success(true);
    }

    // 5. DELETE: Baja de un inmueble validando que no tenga activos asignados (ej. estacionamientos amarrados)
    public async Task<Result<bool>> DeleteUnitAsync(Guid unitId)
    {
        var tenantOrgId = tenantService.GetOrganizationId();

        var unit = await context.Units
            .Include(u => u.LinkedAssets)
            .FirstOrDefaultAsync(u => u.Id == unitId && u.OrganizationId == tenantOrgId);

        if (unit == null) return Result<bool>.Failure("Unidad no encontrada.");

        // Regla de Integridad Física de Negocio: No puedes borrar un apartamento que tenga puestos de estacionamiento asignados.
        if (unit.LinkedAssets.Count > 0)
            return Result<bool>.Failure("Operación denegada: La unidad cuenta con activos fijos privados vinculados (Estacionamientos/Maleteros). Desincronícelos primero.");

        context.Units.Remove(unit);
        await context.SaveChangesAsync();

        // Notificar remoción para cerrar la subcuenta contable histórica
        await publishEndpoint.Publish(new UnitDeletedEvent(unit.Id, unit.Identifier, unit.Aliquot, unit.OwnerEmail, unit.OrganizationId), ctx => StampCorrelationId(ctx));

        return Result<bool>.Success(true);
    }

    // 6. EVENT: Cambio físico de unidad / Mudanza (Ya implementado)
    /// <summary>
    /// REGLA DE NEGOCIO: Ejecuta el traspaso físico de propiedad o mudanza de un usuario, actualizando el inventario y notificando al ecosistema.
    /// </summary>
    public async Task ChangeUnitAsync(Guid userId, Guid oldUnitId, Guid newUnitId)
    {
        var tenantOrgId = tenantService.GetOrganizationId();

        // 1. Cargar las dos unidades involucradas en la operación dentro del alcance Multi-Tenant
        var oldUnit = await context.Units.FirstOrDefaultAsync(u => u.Id == oldUnitId && u.OrganizationId == tenantOrgId);
        var newUnit = await context.Units.FirstOrDefaultAsync(u => u.Id == newUnitId && u.OrganizationId == tenantOrgId);

        if (oldUnit == null || newUnit == null)
            throw new InvalidOperationException("Una o ambas unidades especificadas para el traspaso no existen en su organización.");

        // 2. Ejecutar la mutación de negocio de forma atómica
        // El correo del dueño de la unidad antigua pasa a ser el propietario de la nueva unidad
        string ownerEmail = oldUnit.OwnerEmail;
        newUnit.OwnerEmail = ownerEmail;

        // Opcional: Si tu regla de negocio dicta desvincular al dueño anterior de la unidad vieja, puedes limpiar su campo:
        // oldUnit.OwnerEmail = "disponible@condonet.com"; 

        await context.SaveChangesAsync();

        // 3. Consistencia Eventual: Notificar el cambio físico a RabbitMQ por MassTransit.
        // Esto le permitirá a servicios como 'Booking' cancelar las reservas activas del usuario en la unidad vieja
        // y otorgarle derechos automáticos de reserva en su nuevo departamento.
        await publishEndpoint.Publish(
            new UnitChangedEvent(userId, oldUnitId, newUnitId, DateTime.UtcNow),
            ctx => StampCorrelationId(ctx)
        );
    }

    /// <summary>
    /// Método auxiliar privado para inyectar el CorrelationId en el sobre de MassTransit
    /// </summary>
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
