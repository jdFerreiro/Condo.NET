using CondoNet.Engagement.Core.Entities;
using CondoNet.Engagement.Core.Services;

namespace CondoNet.Engagement.Api.Endpoints;

public static class QuorumEndpoints
{
    public static void MapQuorumEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/quorums");

        group.MapPost("/", async (Quorum quorum, IQuorumService service, HttpContext ctx) =>
        {
            var userId = GetUserId(ctx);
            await service.ValidateAndRecordAsync(quorum, userId);
            return Results.Created($"/quorums/{quorum.Id}", quorum);
        });

        group.MapGet("/meeting/{meetingId}/is-met", async (Guid meetingId, IQuorumService service) =>
        {
            var met = await service.IsQuorumMetAsync(meetingId);
            return Results.Ok(new { isMet = met });
        });
    }

    private static Guid GetUserId(HttpContext ctx)
    {
        // Implementar obtención de usuario autenticado
        return Guid.NewGuid(); // Placeholder
    }
}
