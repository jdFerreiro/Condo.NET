namespace CondoNet.Financial.Core.Entities
{
    // Entidad asociativa para alícuotas variables por sección
    public class UnitAccountSection
    {
        public Guid UnitAccountId { get; set; }
        public virtual UnitAccount UnitAccount { get; set; } = null!;

        public Guid FinancialSubSectionId { get; set; }
        public virtual FinancialSubSection FinancialSubSection { get; set; } = null!;

        public decimal ParticipationPercentage { get; set; } // Ej: 0.0523 (5.23%)
    }
}
