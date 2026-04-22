using CondoNet.Asset.Core.Entities;
using CondoNet.Asset.Core.Interfaces;
using CondoNet.Asset.Infraestructure.Persistence;
using CondoNet.Shared.Asset.DTOs;
using CondoNet.Shared.Asset.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Asset.Infraestructure.Services
{
    public class AssetService(AssetDbContext context, IPublishEndpoint publishEndpoint) : IAssetService
    {
        private readonly AssetDbContext _context = context;
        private readonly IPublishEndpoint _publishEndpoint = publishEndpoint; // 1. Inyectamos el endpoint de publicación

        public async Task<bool> BulkImportUnitsAsync(BulkImportRequest request)
        {
            // 1. Validación de la Regla de Oro (Pág. 22)
            decimal totalAliquot = request.Units.Sum(u => u.Aliquot);

            if (totalAliquot != 100.0000m)
            {
                throw new InvalidOperationException($"La suma de alícuotas es {totalAliquot}%. Debe ser exactamente 100.0000%.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 2. Procesar Torres (Asegurar que existan o crearlas)
                var towerNames = request.Units.Select(u => u.TowerName).Distinct();
                var towers = new List<Tower>();

                foreach (var name in towerNames)
                {
                    var tower = await _context.Towers
                        .FirstOrDefaultAsync(t => t.Name == name && t.CondominiumId == request.CondominiumId);

                    if (tower == null)
                    {
                        tower = new Tower { Name = name, CondominiumId = request.CondominiumId };
                        _context.Towers.Add(tower);
                        await _context.SaveChangesAsync(); // Para obtener el Id de la nueva torre
                    }
                    towers.Add(tower);
                }

                // 3. Crear Unidades
                // Proyectamos a una lista física (.ToList()) para poder iterar y leer los IDs generados después del SaveChanges
                var unitsToInsert = request.Units.Select(dto => new Unit
                {
                    Identifier = dto.Identifier,
                    Aliquot = dto.Aliquot,
                    Type = dto.Type,
                    OwnerEmail = dto.OwnerEmail,
                    TowerId = towers.First(t => t.Name == dto.TowerName).Id,
                }).ToList();

                await _context.Units.AddRangeAsync(unitsToInsert);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // 4. EMISIÓN DEL EVENTO (Consistencia Eventual)
                // Se ejecuta fuera del bloque try/catch transaccional para que solo ocurra tras el commit exitoso

                // Extraemos el OrganizationId de la primera unidad generada por el interceptor de EF Core
                var organizationId = unitsToInsert.First().OrganizationId;

                var eventUnits = unitsToInsert.Select(u => new ImportedUnitDto(
                    u.Id,
                    u.Identifier,
                    u.Aliquot,
                    u.OwnerEmail
                )).ToList();

                await _publishEndpoint.Publish(new UnitsImported(
                    request.CondominiumId,
                    organizationId,
                    eventUnits
                ));

                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }

}