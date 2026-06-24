using CondoNet.Accounting.Core.Interfaces.Services;
using CondoNet.Shared.Accounting.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CondoNet.Accounting.Api.Endpoints
{
    public static class JournalEndpoints
    {
        public static IEndpointRouteBuilder MapJournalEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v1/accounting/journal")
                .WithTags("Accounting - Journal");

            // 1. REGISTRAR COMPROBANTE DE DIARIO MANUAL (PARTIDA DOBLE)
            group.MapPost("", async (
                [FromBody] CreateTransactionRequest request,
                IAccountingService accountingService) =>
            {
                var result = await accountingService.CreateTransactionAsync(request);

                return result.IsSuccess
                    ? Results.Created($"/api/v1/accounting/journal/{result.Value}", result.Value)
                    : Results.BadRequest(result.Error);
            });

            // 2. CONSULTAR DETALLE PROFUNDO DE UN COMPROBANTE ESPECÍFICO
            group.MapGet("{id:guid}", async (
                Guid id,
                IAccountingService accountingService) =>
            {
                var result = await accountingService.GetTransactionByIdAsync(id);

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.NotFound(result.Error);
            });

            // 3. CONSULTAR LIBRO DIARIO MASIVO POR RANGO DE FECHAS
            group.MapGet("", async (
                [FromQuery] DateTime from,
                [FromQuery] DateTime to,
                IAccountingService accountingService) =>
            {
                var result = await accountingService.GetJournalEntriesAsync(from, to);
                return Results.Ok(result.Value);
            });

            // 4. ANULACIÓN Y REVERSIÓN QUIRÚRGICA DE UN COMPROBANTE
            group.MapPost("{id:guid}/void", async (
                Guid id,
                IAccountingService accountingService) =>
            {
                var result = await accountingService.VoidTransactionAsync(id);

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.BadRequest(result.Error);
            });

            return app;
        }
    }
}
