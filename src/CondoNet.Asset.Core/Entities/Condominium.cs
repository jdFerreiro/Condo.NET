using CondoNet.Asset.Core.Interfaces;

namespace CondoNet.Asset.Core.Entities
{
    public class Condominium : ITenantEntity
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; } // Discriminador RLS
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
        public List<Unit> Units { get; set; } = [];
    }
}
