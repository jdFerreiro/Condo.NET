namespace CondoNet.Booking.Core.DTOs
{
    public class BookingMonthStatusDto
    {
        public List<DateTime> FreeDays { get; set; } = new();
        public List<Core.Entities.Booking> Confirmed { get; set; } = new();
        public List<Core.Entities.Booking> Pending { get; set; } = new();
    }
}