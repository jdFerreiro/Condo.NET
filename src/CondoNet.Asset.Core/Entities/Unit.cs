using CondoNet.Asset.Core.Interfaces;
using CondoNet.Shared.Asset;

namespace CondoNet.Asset.Core.Entities
{
    public class Unit : ITenantEntity
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid TowerId { get; set; }
        public string Identifier { get; set; } = null!; // Ej: "402"
        public decimal Aliquot { get; set; } // % de participación
        public UnitType Type { get; set; } // HABITATIONAL, COMMERCIAL
        public string OwnerEmail { get; set; } = null!;

        public Tower Tower { get; set; } = null!;
        public ICollection<Asset> LinkedAssets { get; set; } = []; // Estacionamientos propios
    }


}
