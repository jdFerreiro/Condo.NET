namespace CondoNet.Shared.Booking.Events
{
    public record BookingCreatedIntegrationEvent(
        Guid EventId,
        DateTime OccurredOn,
        Guid BookingId,
        Guid CondoId,
        Guid UserId,
        Guid AssetId,
        decimal TotalPrice,
        decimal RequiredDepositAmount, // <--- Nuevo campo para Accounting
        DateTime Start,
        DateTime End
    );

    public record BookingCancelledIntegrationEvent(
        Guid EventId,
        DateTime OccurredOn,
        Guid BookingId,
        Guid CondoId,
        Guid UserId,
        decimal Price
    );

    public record UserDebtStatusChangedIntegrationEvent(
        Guid EventId,
        DateTime OccurredOn,
        Guid CondoId,
        Guid UserId,
        bool HasOverdueDebt // true = moroso, false = al día
    );

    public record BookingExpirationReminder(
        Guid BookingId
    );

    public record BookingDepositPaidIntegrationEvent(
        Guid EventId,
        DateTime OccurredOn,
        Guid CondoId,
        Guid BookingId,
        Guid UserId,
        decimal AmountPaid
    );
}
