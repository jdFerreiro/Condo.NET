namespace CondoNet.Accounting.Core.Entities
{
    public class TemplateRule
    {
        public Guid Id { get; set; }
        public Guid AccountingTemplateId { get; set; }

        public Guid AccountId { get; set; }
        public RuleMovementType Movement { get; set; } // Debit o Credit

        /// <summary>
        /// Factor multiplicador sobre el Monto Base.
        /// Ejemplos: 
        /// 1.0000 -> 100% (Monto Base completo para la cuenta de Gasto)
        /// 0.1600 -> 16% (Para la cuenta de IVA / Impuesto)
        /// 0.1000 -> 10% (Para retenciones o Fondos de Reserva específicos)
        /// </summary>
        public decimal PercentageFactor { get; set; }

        public AccountingTemplate Template { get; set; } = null!;
    }
    public enum RuleMovementType { Debit = 1, Credit = 2 }
}
