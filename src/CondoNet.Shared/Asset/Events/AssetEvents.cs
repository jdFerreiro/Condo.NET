using CondoNet.Shared.Asset.DTOs;

namespace CondoNet.Shared.Asset.Events
{
    /// <summary>
    /// Emitido cuando se registra un nuevo condominio.
    /// Utilizado por el Billing Service para preparar la facturación base.
    /// </summary>
    public record CondominiumCreated(
        Guid OrganizationId,
        Guid CondominiumId,
        string Name,
        string TaxId,
        string BaseCurrency,
        decimal ReserveFundPercentage
    ) : IntegrationEvent(OrganizationId);

    public record CondominiumUpdated(
        Guid OrganizationId,
        Guid CondominiumId,
        string NewName,
        decimal NewReserveFundPercentage
    ) : IntegrationEvent(OrganizationId);

    /// <summary>
    /// Emitido tras una carga masiva exitosa de unidades.
    /// Utilizado por el Billing Service para generar las cuentas por cobrar iniciales.
    /// </summary>
    public record UnitsImported(
        Guid OrganizationId,
        Guid CondominiumId,
        List<ImportedUnitDto> Units
    ) : IntegrationEvent(OrganizationId);

    public record ImportedUnitDto(
        Guid UnitId,
        string Identifier,
        decimal Aliquot,
        string OwnerEmail
    );

    public record UnitCreated(
        Guid OrganizationId,
        Guid CondominiumId,
        Guid UnitId,
        string Identifier,
        decimal Aliquot,
        string OwnerEmail
    ) : IntegrationEvent(OrganizationId);

    public record UnitAliquotChanged(
        Guid OrganizationId,
        Guid CondominiumId,
        Guid UnitId,
        decimal OldAliquot,
        decimal NewAliquot
    ) : IntegrationEvent(OrganizationId);


    /// <summary>
    /// Emitido cuando un activo (ej: Puesto de estacionamiento) cambia de estado.
    /// Utilizado por el módulo de reservas o mantenimiento.
    /// </summary>
    public record AssetCreated(
        Guid OrganizationId,
        Guid CondominiumId,
        Guid AssetId,
        string Name,
        int Category // Se envía el entero del Enum para no forzar dependencias
    ) : IntegrationEvent(OrganizationId);

    public record AssetLinkedToUnit(
        Guid OrganizationId,
        Guid AssetId,
        Guid UnitId // Unidad a la que ahora pertenece el activo
    ) : IntegrationEvent(OrganizationId);

    public record AssetStatusChanged(
        Guid OrganizationId,
        Guid CondominiumId,
        Guid AssetId,
        int OldStatus,
        int NewStatus
    ) : IntegrationEvent(OrganizationId);

    // Comando que dispara el procesamiento (Enviado por la Minimal API)
    public record ProcessBulkImportCommand(
        Guid ImportId,
        Guid CondominiumId,
        Guid OrganizationId,
        List<UnitBulkImportDto> Units
    );

    // Eventos de progreso de la tarea
    public record ImportStarted(Guid ImportId, Guid OrganizationId);

    public record ImportFailed(Guid ImportId, Guid OrganizationId, string ErrorMessage);

    public record ImportCompleted(Guid ImportId, Guid OrganizationId, int TotalProcessed);

    public record CriticalEquipmentAdded(
        Guid OrganizationId,
        Guid CondominiumId,
        Guid EquipmentId,
        string Name,
        int MaintenanceFrequencyDays
    ) : IntegrationEvent(OrganizationId);
    public record CriticalEquipmentMaintenanceTriggered(
        Guid OrganizationId,
        Guid CondominiumId,
        Guid EquipmentId,
        string EquipmentName,
        DateTime NextDueDate
    ) : IntegrationEvent(OrganizationId);
}
