namespace CondoNet.Engagement.Core.Entities
{
    public class Notification
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public Guid RecipientId { get; set; }
        public bool IsRead { get; set; }
    }
}