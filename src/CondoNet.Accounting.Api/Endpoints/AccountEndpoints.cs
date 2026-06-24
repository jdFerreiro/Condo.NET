using CondoNet.Accounting.Core.Interfaces.Services;
using CondoNet.Shared.Accounting.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CondoNet.Accounting.Api.Endpoints
{
    public static class AccountEndpoints
    {
        public static IEndpointRouteBuilder MapAccountEndpoints(this IEndpointRouteBuilder app)
        {
            // Eliminamos .WithOpenApi() para corregir la advertencia de .NET 9
            var group = app.MapGroup("api/v1/accounting/accounts")
                .WithTags("Accounting - Accounts");

            // 1. REGISTRAR UNA NUEVA CUENTA CONTABLE
            group.MapPost("", async (
                [FromBody] CreateAccountRequest request,
                IAccountingService accountingService) =>
            {
                var result = await accountingService.CreateAccountAsync(request);

                return result.IsSuccess
                    ? Results.Created($"/api/v1/accounting/accounts", result.Value)
                    : Results.BadRequest(result.Error);
            });

            // 2. OBTENER EL PLAN DE CUENTAS COMPLETO DEL CONDOMINIO
            group.MapGet("", async (IAccountingService accountingService) =>
            {
                var result = await accountingService.GetAccountsByCondoAsync();
                return Results.Ok(result.Value);
            });

            // 3. MODIFICAR ATRIBUTOS DESCRIPTIVOS DE UNA CUENTA
            group.MapPut("{id:guid}", async (
                Guid id,
                [FromBody] UpdateAccountRequest request,
                IAccountingService accountingService) =>
            {
                var result = await accountingService.UpdateAccountAsync(id, request);

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.BadRequest(result.Error);
            });

            // 4. CONMUTAR ESTADO (ACTIVAR / DESACTIVAR CUENTA)
            group.MapPatch("{id:guid}/toggle-status", async (
                Guid id,
                [FromQuery] bool isActive,
                IAccountingService accountingService) =>
            {
                var result = await accountingService.ToggleAccountStatusAsync(id, isActive);

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.BadRequest(result.Error);
            });

            group.MapPost("tree", async (
                [FromBody] CreateAccountRequest request,
                IAccountingService accountingService) =>
            {
                // Dispara el servicio con todas las reglas de oro jerárquicas y de codificación
                var result = await accountingService.AddAccountToTreeAsync(request);

                return result.IsSuccess
                    ? Results.Created($"/api/v1/accounting/accounts", result.Value)
                    : Results.BadRequest(result.Error);
            });

            group.MapGet("tree", async (IAccountingService accountingService) =>
            {
                var result = await accountingService.GetAccountTreeAsync();

                // Retorna el árbol contable completamente estructurado en formato jerárquico
                return Results.Ok(result.Value);
            });

            return app;
        }
    }
}
