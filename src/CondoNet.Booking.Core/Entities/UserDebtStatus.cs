namespace CondoNet.Booking.Core.Entities
{
    public class UserDebtStatus
    {
        public Guid Id { get; set; }
        public Guid CondoId { get; set; }
        public Guid UserId { get; set; }
        public bool IsBlocked { get; set; } // true si tiene deudas vencidas
        public DateTime UpdatedAt { get; set; }
    }
}
