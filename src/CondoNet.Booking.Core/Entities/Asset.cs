// CondoNet.Booking.Core.Entities/Asset.cs
namespace CondoNet.Booking.Core.Entities;

public class Asset
{
    public Guid Id { get; set; }
    public Guid CondoId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int ExpirationHours { get; set; } = 24;

    // REGLA DE NEGOCIO: Porcentaje requerido para apartar (ej: 20 para el 20%, 0 = gratis)
    public decimal BookingRequiredPercentage { get; set; } = 0.00m;
}
