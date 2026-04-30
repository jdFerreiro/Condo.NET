
namespace CondoNet.Booking.Core.Entities
{
    public class Booking
    {
        public Guid Id { get; set; }
        public Guid AssetId { get; set; }
        public Guid UserId { get; set; }
        public BookingType Type { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public decimal Price { get; set; }
        public bool IsConfirmed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Recurrence support
        public RecurrenceType RecurrenceType { get; set; } = RecurrenceType.None;
        public int? RecurrenceInterval { get; set; } // e.g., every 2 days/weeks/months
        public DateTime? RecurrenceEnd { get; set; } // null = endless
    }
}
