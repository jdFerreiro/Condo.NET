using CondoNet.Asset.Core.Interfaces;

namespace CondoNet.Asset.Core.Entities
{
    public class Tower : ITenantEntity
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid CondominiumId { get; set; }
        public string Name { get; set; } = null!; // Ej: "Torre A", "Locales"

        public Condominium Condominium { get; set; } = null!;
        public ICollection<Unit> Units { get; set; } = [];
    }

}
