namespace CondoNet.Engagement.Core.Entities
{
    public class Vote
    {
        public Guid Id { get; set; }
        public Guid VotingSessionId { get; set; }
        public Guid VoterId { get; set; }
        public string Option { get; set; } = null!;
        public DateTime CastAt { get; set; }
    }
}