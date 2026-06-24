namespace CondoNet.Shared.Accounting.Events
{
    public interface IIntegrationEvent
    {
        DateTime OccurredOn { get; }
    }

    public record AccountCreatedEvent(
        Guid AccountId,
        Guid CondominiumId,
        string Code,
        string Name,
        string Type,
        DateTime OccurredOn
    ) : IIntegrationEvent;

    public record AccountStatusToggledEvent(
        Guid AccountId,
        bool IsActive,
        DateTime OccurredOn
    ) : IIntegrationEvent;

    // Se dispara al registrar con éxito un comprobante contable equilibrado
    public record AccountingTransactionPostedEvent(
        Guid TransactionId,
        Guid CondominiumId,
        string TransactionNumber,
        decimal TotalAmount,
        DateTime TransactionDate,
        DateTime OccurredOn
    );

    // Se dispara al revertir/anular quirúrgicamente una transacción
    public record AccountingTransactionVoidedEvent(
        Guid TransactionId,
        Guid CondominiumId,
        string TransactionNumber,
        DateTime OccurredOn
    );

    public record BusinessTransactionOccurred(
        Guid CondominiumId,
        int EventType,             // Mapeado a tu BusinessEventType (1 = Factura, 2 = Pago...)
        decimal BaseAmount,        // Monto base de la operación financiera
        string Description,        // Glosa descriptiva para el asiento
        string DocumentReference   // Número de factura o comprobante bancario de origen
    );

}

