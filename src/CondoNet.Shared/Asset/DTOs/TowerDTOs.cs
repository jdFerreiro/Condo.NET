namespace CondoNet.Shared.Asset.DTOs
{
    public record TowerResponse
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public int TotalUnits { get; init; }
        public int TotalHabitationalUnits { get; init; }
        public int TotalCommercialUnits { get; init; }
    }
}
