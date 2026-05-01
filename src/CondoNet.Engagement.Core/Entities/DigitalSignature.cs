namespace CondoNet.Engagement.Core.Entities
{
    public class DigitalSignature
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid DocumentId { get; set; }
        public DateTime SignedAt { get; set; }
        public string SignatureData { get; set; } = null!;
    }
}