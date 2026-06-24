namespace CondoNet.Shared.Accounting.DTOs
{
    // Solicitud para registrar una nueva cuenta contable
    public record CreateAccountRequest(
        string Code,               // Ej: "1.1.01.001"
        string Name,               // Ej: "Caja Chica - Administración"
        int Type,                  // Mapeado del Enum AccountType (Asset = 1, Liability = 2...)
        bool IsTransactional,      // True si puede recibir asientos directos
        Guid? ParentAccountId      // Id de la cuenta de agrupación superior si aplica
    );

    // Solicitud para modificar datos descriptivos de una cuenta
    public record UpdateAccountRequest(
        string Name,
        Guid? ParentAccountId
    );

    // Estructura de respuesta estandarizada para listados del Plan de Cuentas
    public record AccountResponse(
        Guid Id,
        string Code,
        string Name,
        string Type,
        bool IsTransactional,
        decimal CurrentBalance,
        bool IsActive
    );

    // Solicitud para registrar un Comprobante de Diario Completo
    public record CreateTransactionRequest(
        string Description,
        DateTime Date,
        List<CreateEntryRequest> Entries // Exige adjuntar la lista de renglones balanceados
    );

    // Renglón o Apunte quirúrgico individual que viaja dentro de la solicitud
    public record CreateEntryRequest(
        Guid AccountId,
        decimal Debit,  // Debe
        decimal Credit, // Haber
        string? Reference
    );

    // Resumen ligero de transacciones para grillas de consulta masiva (Libro Diario)
    public record TransactionSummaryResponse(
        Guid Id,
        string Number,
        string Description,
        DateTime Date,
        string Status,
        decimal TotalAmount, // Sumatoria de los débitos del comprobante
        string CreatedBy
    );

    // Detalle quirúrgico profundo de un comprobante con todos sus renglones
    public record TransactionDetailResponse(
        Guid Id,
        string Number,
        string Description,
        DateTime Date,
        string Status,
        string CreatedBy,
        List<EntryDetailResponse> Entries
    );

    // Detalle de cada renglón devuelto por la API
    public record EntryDetailResponse(
        Guid Id,
        Guid AccountId,
        string AccountCode,
        string AccountName,
        decimal Debit,
        decimal Credit,
        string? Reference
    );

    // Estructura para el Balance General (Activo = Pasivo + Patrimonio)
    public record BalanceSheetResponse(
        List<BalanceItemDto> Assets,
        List<BalanceItemDto> Liabilities,
        List<BalanceItemDto> Equity,
        decimal TotalAssets,
        decimal TotalLiabilitiesAndEquity
    );

    // Estructura para el Estado de Resultados (Ingresos - Egresos = Utilidad)
    public record IncomeStatementResponse(
        List<BalanceItemDto> Revenues,
        List<BalanceItemDto> Expenses,
        decimal TotalRevenues,
        decimal TotalExpenses,
        decimal NetIncome // Utilidad o Pérdida neta del ejercicio
    );

    // Sub-DTO reutilizable para transportar los saldos consolidados por cuenta en los reportes
    public record BalanceItemDto(
        string AccountCode,
        string AccountName,
        decimal Balance
    );

    public record ProcessAutomatedEntryRequest(
        Guid CondominiumId,
        int EventType,             // Mapeado de BusinessEventType
        decimal BaseAmount,        // El monto principal de la operación (Monto Base)
        string Description,        // Concepto del asiento
        string DocumentReference   // Nro de factura, recibo o transferencia origen
    );

    public record AccountNodeDto(
        Guid Id,
        string Code,
        string Name,
        string Type,
        bool IsTransactional,
        decimal CurrentBalance,
        bool IsActive,
        List<AccountNodeDto> Children // Lista recursiva para almacenar los nodos hijos
    );
}

