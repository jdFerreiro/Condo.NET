namespace CondoNet.Accounting.Core.Entities
{
    public class AccountingTemplate
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid CondominiumId { get; set; }

        public BusinessEventType EventType { get; set; }
        public string Name { get; set; } = null!; // Ej: "Gasto Proveedor con IVA y Fondo de Reserva"
        public string Description { get; set; } = null!;

        // Una plantilla tiene N reglas para permitir asientos 1:N, N:1 o M:N
        public List<TemplateRule> Rules { get; set; } = [];
    }

    public enum BusinessEventType
    {
        MonthlyBillingGenerated = 1, // Emisión del recibo de condominio
        InvoicePaymentReceived = 2,  // Pago de recibo por parte de un propietario
        SupplierExpenseRecorded = 3, // Registro de un gasto de proveedor (Luz, Agua, Conserje)
        BankFeeIncurred = 4          // Comisión bancaria automática
    }

}
