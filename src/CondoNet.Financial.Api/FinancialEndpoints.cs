using CondoNet.Financial.Core.Dtos;
using CondoNet.Financial.Core.Interfaces;

namespace CondoNet.Financial.Api
{
    public static class FinancialEndpoints
    {
        public static void MapFinancialEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/financial")
                           .WithTags("Gestión de finanzas"); var api = app.MapGroup("/api/financial");

            // Endpoints de Transacciones
            api.MapPost("/transactions", async (CreateTransactionDto dto, IFinancialService service) =>
                Results.Ok(await service.RegisterTransactionAsync(dto)));

            // Endpoints de Consultas
            api.MapGet("/balance/{accountId}", async (int accountId, IFinancialService service) =>
                Results.Ok(new { Balance = await service.GetCurrentBalanceAsync(accountId) }));

            // Endpoints de Facturación
            api.MapPost("/invoices/generate", async (int accountId, decimal amount, IFinancialService service) =>
                Results.Ok(await service.GenerateMonthlyInvoiceAsync(accountId, amount, "Cuota de mantenimiento")));
        }
    }
}
