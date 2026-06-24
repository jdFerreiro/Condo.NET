using CondoNet.Shared.Asset.DTOs;

namespace CondoNet.Shared.Asset.Events
{
    public record ProcessBulkImportCommand(
        Guid ImportId,
        Guid OrganizationId,
        Guid CondominiumId,
        List<UnitBulkImportDto> Units // Reutiliza tu DTO exacto de arriba
    );
    public record ImportStarted(
        Guid ImportId,
        Guid OrganizationId
    );

    // 2. NOTIFICA EL ÉXITO ABSOLUTO E INFORMA LA CANTIDAD DE UNIDADES PROCESADAS
    public record ImportCompleted(
        Guid ImportId,
        Guid OrganizationId,
        int ProcessedRowsCount
    );

    // 3. NOTIFICA EL FALLO (YA SEA POR DESCUADRE DE ALÍCUOTAS O EXCEPCIONES DE BASE DE DATOS)
    public record ImportFailed(
        Guid ImportId,
        Guid OrganizationId,
        string ErrorMessage
    );
}
