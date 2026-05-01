using CondoNet.Engagement.Core.Entities;
using CondoNet.Engagement.Core.Services;

namespace CondoNet.Engagement.Api.Endpoints;

public static class DigitalSignatureEndpoints
{
    public static void MapDigitalSignatureEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/digitalsignatures");

        group.MapPost("/", async (DigitalSignature signature, IDigitalSignatureService service, HttpContext ctx) =>
        {
            var userId = GetUserId(ctx);
            await service.ValidateAndSignAsync(signature, userId);
            return Results.Created($"/digitalsignatures/{signature.Id}", signature);
        });
    }

    private static Guid GetUserId(HttpContext ctx)
    {
        // Implementar obtención de usuario autenticado
        return Guid.NewGuid(); // Placeholder
    }
}
