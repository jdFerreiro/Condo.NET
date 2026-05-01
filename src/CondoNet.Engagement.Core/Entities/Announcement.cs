namespace CondoNet.Engagement.Core.Entities
{
    public class Announcement
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public bool IsActive { get; set; }
    }
}