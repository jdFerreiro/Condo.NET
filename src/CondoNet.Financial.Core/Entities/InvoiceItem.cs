namespace CondoNet.Financial.Core.Entities
{
    public class InvoiceItem : BaseEntity
    {
        public Guid InvoiceId { get; set; }
        public string Description { get; set; } = null!; // Ej: "Gasto Común - Torre A"
        public decimal Amount { get; set; }
        public Guid? RelatedSubSectionId { get; set; } // De qué sección viene este cobro
    }
}
