using CondoNet.Asset.Core.Interfaces;
using CondoNet.Asset.Infrastructure.Persistence;
using CondoNet.Shared.Asset.DTOs;
using CondoNet.Shared.Asset.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Asset.Infrastructure.Services;

public class UnitService(AssetDbContext context, IPublishEndpoint publishEndpoint) : IUnitService
{
    private readonly AssetDbContext _context = context;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

    public async Task<List<UnitResponse>> GetUnitsByOwnerAsync(string ownerId)
    {
        return await _context.Units
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

    public async Task ChangeUnitAsync(Guid userId, Guid oldUnitId, Guid newUnitId)
    {
        // Emitir evento de cambio de unidad
        await _publishEndpoint.Publish(new UnitChangedEvent
        {
            UserId = userId,
            OldUnitId = oldUnitId,
            NewUnitId = newUnitId,
            ChangedAt = DateTime.UtcNow
        });
        // Aquí podrías agregar lógica adicional si es necesario
    }
}
