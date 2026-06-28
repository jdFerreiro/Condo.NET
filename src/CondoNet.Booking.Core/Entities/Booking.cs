
using CondoNet.Shared.Booking.Enums;

namespace CondoNet.Booking.Core.Entities
{
    public class Booking
    {
        public Guid Id { get; set; }
        public Guid CondoId { get; set; } // <--- Filtro de Tenant
        public Guid AssetId { get; set; }
        public Guid UserId { get; set; }
        public BookingType Type { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public decimal Price { get; set; }
        public bool IsConfirmed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Recurrencia
        public RecurrenceType RecurrenceType { get; set; } = RecurrenceType.None;
        public int? RecurrenceInterval { get; set; }
        public DateTime? RecurrenceEnd { get; set; }
    }
}
