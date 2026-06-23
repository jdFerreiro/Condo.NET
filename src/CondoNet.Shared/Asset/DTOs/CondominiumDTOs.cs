using System.ComponentModel.DataAnnotations;

namespace CondoNet.Shared.Asset.DTOs
{
    public record CreateCondominiumRequest
    {
        public Guid OrganizationId { get; set; } // Discriminador RLS
        [MaxLength(500)]
        public string Name { get; set; } = null!;
        [MaxLength(50)]
        public string TaxId { get; set; } = null!;
        public string Address { get; set; } = null!;
        public decimal ReserveFundPercentage { get; set; }

    }

    public record CreateCondominiumResponse
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; } // Discriminador RLS
        public string Name { get; set; } = null!;
        public string TaxId { get; set; } = null!;
        public string Address { get; set; } = null!;
        public decimal ReserveFundPercentage { get; set; }
    }

    public record UpdateCondominiumRequest
    {
        [MaxLength(500)]
        public string Name { get; set; } = null!;
        [MaxLength(50)]
        public string TaxId { get; set; } = null!;
        public string Address { get; set; } = null!;
        public decimal ReserveFundPercentage { get; set; }

    }

    public record UpdateCondominiumResponse
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; } // Discriminador RLS
        public string Name { get; set; } = null!;
        public string TaxId { get; set; } = null!;
        public string Address { get; set; } = null!;
        public decimal ReserveFundPercentage { get; set; }
    }

    public record GetCondominiumResponse
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; } // Discriminador RLS
        public string Name { get; set; } = null!;
        public string TaxId { get; set; } = null!;
        public string Address { get; set; } = null!;
        public decimal ReserveFundPercentage { get; set; }

        public GetCondominiumResponse(Guid id, Guid organizationId, string name, string taxId, string address, decimal reserveFundPercentage)
        {
            Id = id;
            OrganizationId = organizationId;
            Name = name;
            TaxId = taxId;
            Address = address;
            ReserveFundPercentage = reserveFundPercentage;
        }
    }

}
