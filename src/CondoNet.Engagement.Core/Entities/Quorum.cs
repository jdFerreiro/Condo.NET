namespace CondoNet.Engagement.Core.Entities
{
    public class Quorum
    {
        public Guid Id { get; set; }
        public Guid MeetingId { get; set; }
        public int PresentCount { get; set; }
        public int RequiredCount { get; set; }
        public DateTime RecordedAt { get; set; }
    }
}