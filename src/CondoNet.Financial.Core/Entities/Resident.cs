namespace CondoNet.Financial.Core.Entities
{
    public class Resident : BaseEntity
    {
        public Guid UnitAccountId { get; set; }
        public UnitAccount UnitAccount { get; set; } = null!;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int FraudWarnings { get; set; }
        public TrustLevel TrustLevel { get; set; }
        public DateTime? LastFraudDate { get; set; }
    }

    public enum TrustLevel
    {
        Normal,
        Restricted,
        Blacklisted
    }
}
