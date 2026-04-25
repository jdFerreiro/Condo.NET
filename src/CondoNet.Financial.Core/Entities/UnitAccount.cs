namespace CondoNet.Financial.Core.Entities
{
    public class UnitAccount : BaseEntity
    {
        public string ExternalUnitId { get; set; } = string.Empty; // ID de la torre/apartamento
        public decimal ParticipationPercentage { get; set; } // Alícuota

        // Datos espejeados de la Blockchain para lectura rápida
        public decimal CurrentDebt { get; set; }
        public decimal CreditBalance { get; set; }
        public string? LastBlockchainTransactionHash { get; set; }

        // Relaciones
        public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    }
}
