using CondoNet.Asset.Core.Interfaces;

namespace CondoNet.Asset.Core.Entities
{
    public class CriticalEquipment : ITenantEntity
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid CondominiumId { get; set; }

        public string Name { get; set; } = null!; // Ej: "Bomba Hidroneumática 1"
        public string Brand { get; set; } = null!;
        public string Model { get; set; } = null!;
        public DateTime InstallationDate { get; set; }
        public int MaintenanceFrequencyDays { get; set; } // Ej: cada 30 días
        public string ManualUrl { get; set; } = null!;
    }
}
