using CondoNet.Asset.Core.Entities;
using CondoNet.Shared.Asset;
using CondoNet.Shared.Asset.DTOs;

namespace CondoNet.Asset.Api.Mappings
{
    public static class UnitMappingExtensions
    {
        /// <summary>
        /// Mapea un DTO de comando a una entidad de base de datos Unit.
        /// </summary>
        public static Unit ToEntity(this UnitBulkImportDto dto, Guid organizationId, Guid towerId)
        {
            return new Unit
            {
                Id = Guid.NewGuid(), // Generamos el ID de la unidad
                OrganizationId = organizationId,
                TowerId = towerId,
                Identifier = dto.Identifier,
                Aliquot = dto.Aliquot,
                // Casteamos el entero que viene de la cola al Enum local del microservicio
                Type = (UnitType)dto.Type,
                OwnerEmail = dto.OwnerEmail
            };
        }
    }
}
