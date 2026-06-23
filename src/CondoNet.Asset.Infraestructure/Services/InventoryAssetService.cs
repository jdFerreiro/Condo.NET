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

public class InventoryAssetService(
    AssetDbContext context,
    ITenantService tenantService,
    IPublishEndpoint publishEndpoint,
    IHttpContextAccessor httpContextAccessor) : IInventoryAssetService
{
    // ==========================================
    // SECCIÓN A: GESTIÓN DE BIENES (ASSETS PRIVADOS Y ÁREAS COMUNES)
    // ==========================================

    public async Task<Result<Guid>> RegisterAssetAsync(RegisterAssetRequest request)
    {
        var tenantOrgId = tenantService.GetOrganizationId();
        var tenantCondoId = tenantService.GetCondominiumId();

        var isDuplicate = await context.Assets.AnyAsync(a =>
            a.Name.Trim().ToUpper() == request.Name.Trim().ToUpper() && a.CondominiumId == tenantCondoId);
        if (isDuplicate)
            return Result<Guid>.Failure($"El activo o área '{request.Name}' ya existe.");

        if (request.LinkedUnitId.HasValue)
        {
            var unitExists = await context.Units.AnyAsync(u => u.Id == request.LinkedUnitId.Value);
            if (!unitExists) return Result<Guid>.Failure("La unidad inmobiliaria a vincular no existe.");
        }

        var newAsset = new Core.Entities.Asset
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Category = (AssetType)request.Category,
            IsRentable = request.IsRentable,
            DefaultRentalPrice = request.DefaultRentalPrice,
            LinkedUnitId = request.LinkedUnitId,
            Status = AssetStatus.Available,
            CondominiumId = tenantCondoId,
            OrganizationId = tenantOrgId
        };

        context.Assets.Add(newAsset);
        await context.SaveChangesAsync();

        if (newAsset.IsRentable)
        {
            await publishEndpoint.Publish(new RentableAssetCreatedEvent(
                newAsset.Id, newAsset.Name, newAsset.DefaultRentalPrice ?? 0, newAsset.Category.ToString(), tenantCondoId, tenantOrgId),
                ctx => StampCorrelationId(ctx));
        }

        return Result<Guid>.Success(newAsset.Id);
    }

    public async Task<List<AssetInventoryResponse>> GetActiveAssetsAsync()
    {
        var tenantCondoId = tenantService.GetCondominiumId();
        return await context.Assets
            .AsNoTracking()
            .Where(a => a.CondominiumId == tenantCondoId)
            .Select(a => new AssetInventoryResponse(a.Id, a.Name, a.Category.ToString(), a.Status.ToString(), a.LinkedUnitId))
            .ToListAsync();
    }

    public async Task<Result<AssetInventoryResponse>> GetAssetByIdAsync(Guid assetId)
    {
        var tenantCondoId = tenantService.GetCondominiumId();

        var asset = await context.Assets
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == assetId && a.CondominiumId == tenantCondoId);

        if (asset == null)
            return Result<AssetInventoryResponse>.Failure("Activo no encontrado o no pertenece a su condominio.");

        return Result<AssetInventoryResponse>.Success(new AssetInventoryResponse(
            asset.Id, asset.Name, asset.Category.ToString(), asset.Status.ToString(), asset.LinkedUnitId));
    }

    public async Task<Result<bool>> UpdateAssetAsync(Guid assetId, UpdateAssetRequest request)
    {
        var tenantCondoId = tenantService.GetCondominiumId();

        var asset = await context.Assets.FirstOrDefaultAsync(a => a.Id == assetId && a.CondominiumId == tenantCondoId);
        if (asset == null) return Result<bool>.Failure("Activo no encontrado.");

        bool priceChanged = asset.DefaultRentalPrice != request.DefaultRentalPrice || asset.IsRentable != request.IsRentable;

        asset.Name = request.Name.Trim();
        asset.IsRentable = request.IsRentable;
        asset.DefaultRentalPrice = request.DefaultRentalPrice;

        await context.SaveChangesAsync();

        if (priceChanged)
        {
            await publishEndpoint.Publish(new RentableAssetPriceChangedEvent(
                asset.Id, asset.IsRentable, asset.DefaultRentalPrice ?? 0, tenantCondoId, tenantService.GetOrganizationId(), DateTime.UtcNow),
                ctx => StampCorrelationId(ctx));
        }

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> LinkAssetToUnitAsync(Guid assetId, Guid? linkedUnitId)
    {
        var tenantCondoId = tenantService.GetCondominiumId();

        var asset = await context.Assets.FirstOrDefaultAsync(a => a.Id == assetId && a.CondominiumId == tenantCondoId);
        if (asset == null) return Result<bool>.Failure("Activo no encontrado.");

        if (linkedUnitId.HasValue)
        {
            var unitExists = await context.Units.AnyAsync(u => u.Id == linkedUnitId.Value && u.Tower.CondominiumId == tenantCondoId);
            if (!unitExists) return Result<bool>.Failure("La unidad inmobiliaria objetivo no pertenece a este condominio o no existe.");

            asset.LinkedUnitId = linkedUnitId;
            asset.Status = AssetStatus.Rented;
        }
        else
        {
            asset.LinkedUnitId = null;
            asset.Status = AssetStatus.Available;
        }

        await context.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> UpdateAssetStatusAsync(Guid assetId, int newStatusValue)
    {
        var tenantCondoId = tenantService.GetCondominiumId();

        var asset = await context.Assets.FirstOrDefaultAsync(a => a.Id == assetId && a.CondominiumId == tenantCondoId);
        if (asset == null) return Result<bool>.Failure("Activo no encontrado.");

        asset.Status = (AssetStatus)newStatusValue;
        await context.SaveChangesAsync();

        await publishEndpoint.Publish(new AssetStatusChangedEvent(
            asset.Id, asset.Status.ToString(), tenantCondoId, tenantService.GetOrganizationId(), DateTime.UtcNow),
            ctx => StampCorrelationId(ctx));

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> DeleteAssetAsync(Guid assetId)
    {
        var tenantCondoId = tenantService.GetCondominiumId();

        var asset = await context.Assets.FirstOrDefaultAsync(a => a.Id == assetId && a.CondominiumId == tenantCondoId);
        if (asset == null) return Result<bool>.Failure("Activo no encontrado.");

        if (asset.LinkedUnitId.HasValue)
            return Result<bool>.Failure("Operación denegada: El activo se encuentra vinculado privadamente a un inmueble.");

        context.Assets.Remove(asset);
        await context.SaveChangesAsync();

        return Result<bool>.Success(true);
    }

    // ==========================================
    // SECCIÓN B: GESTIÓN DE EQUIPAMIENTO CRÍTICO (MAQUINARIA)
    // ==========================================

    public async Task<Result<Guid>> RegisterCriticalEquipmentAsync(RegisterEquipmentRequest request)
    {
        var tenantOrgId = tenantService.GetOrganizationId();
        var tenantCondoId = tenantService.GetCondominiumId();

        var isDuplicate = await context.CriticalEquipments.AnyAsync(e =>
            e.Name.Trim().ToUpper() == request.Name.Trim().ToUpper() && e.CondominiumId == tenantCondoId);
        if (isDuplicate)
            return Result<Guid>.Failure($"El equipo crítico '{request.Name}' ya se encuentra registrado en este condominio.");

        var equipment = new CriticalEquipment
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Brand = request.Brand.Trim(),
            Model = request.Model.Trim(),
            InstallationDate = request.InstallationDate,
            MaintenanceFrequencyDays = request.MaintenanceFrequencyDays,
            ManualUrl = request.ManualUrl.Trim(),
            CondominiumId = tenantCondoId,
            OrganizationId = tenantOrgId
        };

        context.CriticalEquipments.Add(equipment);
        await context.SaveChangesAsync();

        await publishEndpoint.Publish(new CriticalEquipmentRegisteredEvent(
            equipment.Id, equipment.Name, equipment.MaintenanceFrequencyDays, tenantCondoId, tenantOrgId),
            ctx => StampCorrelationId(ctx));

        return Result<Guid>.Success(equipment.Id);
    }

    public async Task<List<EquipmentResponse>> GetCriticalEquipmentsAsync()
    {
        var tenantCondoId = tenantService.GetCondominiumId();
        return await context.CriticalEquipments
            .AsNoTracking()
            .Where(e => e.CondominiumId == tenantCondoId)
            .Select(e => new EquipmentResponse(e.Id, e.Name, e.Brand, e.Model, e.InstallationDate, e.MaintenanceFrequencyDays))
            .ToListAsync();
    }

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
