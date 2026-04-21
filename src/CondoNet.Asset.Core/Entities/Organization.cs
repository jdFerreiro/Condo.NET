using CondoNet.Shared.Asset;
using System.ComponentModel.DataAnnotations;

namespace CondoNet.Asset.Core.Entities
{
    public class Organization
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = null!; // Nombre de la Administradora

        [Required]
        [MaxLength(50)]
        public string TaxId { get; set; } = null!; // RIF, NIT o RUT

        public string? LogoUrl { get; set; }

        [Required]
        [MaxLength(3)]
        public string BaseCurrency { get; set; } = "USD"; // Moneda base para reportes

        // Datos de contacto de la administración central
        public string ContactEmail { get; set; } = null!;
        public string? PhoneNumber { get; set; }

        // Suscripción al Software (Pág. 58)
        public SubscriptionPlan Plan { get; set; } = SubscriptionPlan.Free;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Relaciones: Una organización tiene N condominios
        public virtual ICollection<Condominium> Condominiums { get; set; } = [];
    }
}
