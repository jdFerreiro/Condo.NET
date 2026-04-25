namespace CondoNet.Shared.Asset.DTOs;

public class UnitResponse
{
    public Guid Id { get; set; }
    public Guid TowerId { get; set; }
    public string Identifier { get; set; } = null!;
    public string? Floor { get; set; }
    public decimal? AreaSquareMeters { get; set; }
    public string? Alias { get; set; }
    public decimal Aliquot { get; set; }
    public string OwnerEmail { get; set; } = null!;
    public string? Type { get; set; }
}
