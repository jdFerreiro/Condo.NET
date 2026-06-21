namespace CondoNet.Shared.Events.Payments;

public class InvoiceRegisteredEvent
{
    public Guid CorrelationId { get; init; }
    public Guid InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public DateTime RegisteredAt { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string VendorName { get; set; } = string.Empty;
    // Puedes agregar más campos según tu dominio
}
