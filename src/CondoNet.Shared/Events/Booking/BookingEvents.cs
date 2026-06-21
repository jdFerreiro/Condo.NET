namespace CondoNet.Shared.Events.Booking
{
    public class BookingCreatedEvent
    {
        public Guid CorrelationId { get; init; }
        public Guid BookingId { get; set; }
        public Guid AssetId { get; set; }
        public Guid UserId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; } = "PendingPayment";
    }

    public class BookingConfirmedEvent
    {
        public Guid CorrelationId { get; init; }
        public Guid BookingId { get; set; }
        public Guid AssetId { get; set; }
        public Guid UserId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; } = "Confirmed";
    }
}