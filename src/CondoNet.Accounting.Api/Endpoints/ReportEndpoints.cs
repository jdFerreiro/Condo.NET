using CondoNet.Accounting.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace CondoNet.Accounting.Api.Endpoints
{
    public static class ReportEndpoints
    {
        public static IEndpointRouteBuilder MapReportEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v1/accounting/reports")
                .WithTags("Accounting - Financial Reports");

            // 1. EXTRAER BALANCE GENERAL (ACTIVO = PASIVO + PATRIMONIO)
            group.MapGet("balance-sheet", async (IAccountingService accountingService) =>
            {
                var result = await accountingService.GetBalanceSheetAsync();

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.BadRequest(result.Error);
            });

            // 2. EXTRAER ESTADO DE RESULTADOS (INGRESOS - EGRESOS = UTILIDAD)
            group.MapGet("income-statement", async (
                [FromQuery] DateTime from,
                [FromQuery] DateTime to,
                IAccountingService accountingService) =>
            {
                var result = await accountingService.GetIncomeStatementAsync(from, to);

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.BadRequest(result.Error);
            });

            return app;
        }
    }
}
