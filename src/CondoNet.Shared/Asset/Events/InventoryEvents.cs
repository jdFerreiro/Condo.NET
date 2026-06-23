namespace CondoNet.Shared.Asset.Events
{
    public record AssetStatusChangedEvent
    {
        public Guid AssetId { get; init; }
        public Guid CondoId { get; set; }
        public Guid OrganizationId { get; set; }
        public string NewStatus { get; init; } = null!;
        public DateTime ChangedAt { get; init; }
        public AssetStatusChangedEvent(Guid assetId, string newStatus, Guid condoId, Guid organizationId, DateTime changedAt)
        {
            AssetId = assetId;
            NewStatus = newStatus;
            CondoId = condoId;
            OrganizationId = organizationId;
            ChangedAt = changedAt;
        }
    }

    public record RentableAssetCreatedEvent
    {
        public Guid AssetId { get; init; }
        public string Name { get; set; }
        public decimal DefaultRentalPrice { get; set; }
        public string AssetType { get; set; }
        public Guid CondoId { get; set; }
        public Guid OrganizationId { get; init; }
        public RentableAssetCreatedEvent(Guid assetId, string name, decimal defaultRentalPrice, string assetType, Guid condoId, Guid organizationId)
        {
            AssetId = assetId;
            Name = name;
            DefaultRentalPrice = defaultRentalPrice;
            AssetType = assetType;
            CondoId = condoId;
            OrganizationId = organizationId;
        }
    }

    public record RentableAssetPriceChangedEvent
    {
        public Guid AssetId { get; init; }
        public bool IdRentable { get; set; }
        public decimal NewPrice { get; init; }
        public Guid CondoId { get; set; }
        public Guid OrganizationId { get; set; }
        public DateTime ChangedAt { get; init; }

        public RentableAssetPriceChangedEvent(Guid assetId, bool isRentable, decimal newPrice, Guid condoId, Guid organizationId, DateTime changedAt)
        {
            AssetId = assetId;
            IdRentable = isRentable;
            NewPrice = newPrice;
            CondoId = condoId;
            OrganizationId = organizationId;
            ChangedAt = changedAt;
        }
    }

    public record CriticalEquipmentRegisteredEvent
    {
        public Guid EquipmentId { get; init; }
        public string Name { get; init; } = null!;
        public int MaintenanceFrequencyDays { get; init; }
        public Guid CondominiumId { get; init; }
        public Guid OrganizationId { get; init; }

        public CriticalEquipmentRegisteredEvent(Guid equipmentId, string name, int maintenanceFrequencyDays, Guid condominiumId, Guid organizationId)
        {
            EquipmentId = equipmentId;
            Name = name;
            MaintenanceFrequencyDays = maintenanceFrequencyDays;
            CondominiumId = condominiumId;
            OrganizationId = organizationId;
        }
    }
}
