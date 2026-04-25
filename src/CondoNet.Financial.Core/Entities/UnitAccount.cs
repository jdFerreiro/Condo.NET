namespace CondoNet.Financial.Core.Entities
{
    public class UnitAccount : BaseEntity
    {
        public string ExternalUnitId { get; set; } = null!; // Referencia a microservicio Asset
        public string OwnerName { get; set; } = null!;
        public string? WalletAddress { get; set; } // Dirección Blockchain del propietario

        public decimal CurrentDebt { get; set; }
        public decimal CreditBalance { get; set; }

        public virtual ICollection<UnitAccountSection> SectionAssignments { get; set; } = new List<UnitAccountSection>();
        public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    }
}
