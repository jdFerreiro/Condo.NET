using CondoNet.Asset.Core.Interfaces;

namespace CondoNet.Asset.Core.Entities
{
    public class Asset : ITenantEntity
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid CondominiumId { get; set; }

        public string Name { get; set; } = null!; // Ej: "Puesto E-12" o "Salón de Fiestas"
        public AssetType Category { get; set; }

        // Lógica de Negocio (Pág. 7 y 41)
        public bool IsRentable { get; set; } // ¿Se puede alquilar a externos/terceros?
        public decimal? DefaultRentalPrice { get; set; }

        // Vinculación (Pág. 74)
        public Guid? LinkedUnitId { get; set; } // Si es nulo, pertenece a la Junta/Común
        public AssetStatus Status { get; set; }

        public Unit? LinkedUnit { get; set; }
        public Condominium Condominium { get; set; } = null!;
    }

    public enum AssetType { ParkingSpot = 1, SocialArea = 2, Gym = 3, Storage = 4 }
    public enum AssetStatus { Available = 1, Rented = 2, Maintenance = 3, OutOfService = 4 }
}
