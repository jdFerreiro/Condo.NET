namespace CondoNet.Financial.Core.Entities
{
    public class CurrencyAdjustmentLog : BaseEntity
    {
        public EntityType EntityType { get; set; }
        public Guid EntityId { get; set; }
        public Guid? ReferenceInvoiceId { get; set; }
        public decimal PreviousRate { get; set; }
        public decimal NewRate { get; set; }
        public decimal AmountVesDiff { get; set; }
        public CurrencyAdjustmentReason Reason { get; set; }
    }

    public enum CurrencyAdjustmentReason
    {
        DailyRevaluation,
        PaymentDelay,
        ManualCorrection,
        RateUpdate
    }

    public enum EntityType
    {
        Unit,
        Fund,
        Rate
    }
}
