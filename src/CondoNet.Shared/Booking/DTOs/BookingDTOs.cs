using CondoNet.Shared.Booking.Enums;

namespace CondoNet.Shared.Booking.DTOs
{
    public record CreateBookingRequest(
        Guid AssetId,
        DateTime Start,
        DateTime End,
        BookingType Type,
        RecurrenceType RecurrenceType = RecurrenceType.None,
        int? RecurrenceInterval = null,
        DateTime? RecurrenceEnd = null
    );

    public record RecurrenceDetailsDto(
        RecurrenceType Type,
        int Interval,
        DateTime? End
    );

    public record BookingResponse(
        Guid Id,
        Guid AssetId,
        string AssetName,
        Guid UserId,
        BookingType Type,
        DateTime Start,
        DateTime End,
        decimal Price,
        bool IsConfirmed,
        RecurrenceDetailsDto? Recurrence
    );

    public record UpdateBookingRequest(
        DateTime Start,
        DateTime End
    );

    public record DayStatusDto(
        int Day,
        string Status, // "Available", "Partial", "Full"
        int BookingsCount
    );

    public record MonthStatusResponse(
        Guid AssetId,
        int Year,
        int Month,
        List<DayStatusDto> Calendar
    );
}
