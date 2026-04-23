using CondoNet.Asset.Core.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace CondoNet.Asset.Core.Entities
{
    public class Condominium : ITenantEntity
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; } // Discriminador RLS
        [MaxLength(500)]
        public string Name { get; set; } = null!;
        [MaxLength(50)]
        public string TaxId { get; set; } = null!;
        public string Address { get; set; } = null!;
        public decimal ReserveFundPercentage { get; set; }
        public List<Unit> Units { get; set; } = [];
    }
}
