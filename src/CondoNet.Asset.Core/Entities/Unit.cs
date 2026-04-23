using CondoNet.Asset.Core.Interfaces;
using CondoNet.Shared.Asset;
using System.ComponentModel.DataAnnotations;

namespace CondoNet.Asset.Core.Entities
{
    public class Unit : ITenantEntity
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid TowerId { get; set; }
        public string Identifier { get; set; } = null!; // Ej: "402"
        [MaxLength(10)]
        public string Floor { get; set; } = null!; // Ej: "4", "PB", "S1"
        // Área en metros cuadrados (Útil para justificar alícuotas o avalúos)
        public decimal? AreaSquareMeters { get; set; }
        // Nombre alternativo o alias descriptivo (Ej: "Oficina de Administración")
        public string? Alias { get; set; }
        public decimal Aliquot { get; set; } // % de participación
        public UnitType Type { get; set; } // HABITATIONAL, COMMERCIAL
        public string OwnerEmail { get; set; } = null!;

        public Tower Tower { get; set; } = null!;
        public ICollection<Asset> LinkedAssets { get; set; } = []; // Estacionamientos propios
    }


}
