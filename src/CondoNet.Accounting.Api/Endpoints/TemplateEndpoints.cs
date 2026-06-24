using CondoNet.Accounting.Core.Interfaces;
using CondoNet.Shared.Accounting.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CondoNet.Accounting.Api.Endpoints
{
    public static class TemplateEndpoints
    {
        public static IEndpointRouteBuilder MapTemplateEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/v1/accounting/templates")
                .WithTags("Accounting - Automaton Templates");

            // 1. CREATE: Registrar una nueva plantilla contable con sus reglas de distribución e impuestos
            group.MapPost("", async (
                [FromBody] CreateTemplateRequest request,
                IAccountingTemplateService templateService) =>
            {
                var result = await templateService.CreateTemplateAsync(request);
                return result.IsSuccess
                    ? Results.Created($"/api/v1/accounting/templates/{result.Value}", result.Value)
                    : Results.BadRequest(result.Error);
            });

            // 2. READ ALL: Obtener el listado completo de configuraciones del autómata para el condominio
            group.MapGet("", async (IAccountingTemplateService templateService) =>
            {
                var result = await templateService.GetTemplatesByCondoAsync();
                return Results.Ok(result.Value);
            });

            // 3. DELETE: Eliminar una plantilla de traducción automática de eventos
            group.MapDelete("{id:guid}", async (
                Guid id,
                IAccountingTemplateService templateService) =>
            {
                var result = await templateService.DeleteTemplateAsync(id);
                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.BadRequest(result.Error);
            });

            return app;
        }
    }
}
