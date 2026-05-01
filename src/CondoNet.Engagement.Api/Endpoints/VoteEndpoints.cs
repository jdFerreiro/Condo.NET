using CondoNet.Engagement.Core.Entities;
using CondoNet.Engagement.Core.Services;

namespace CondoNet.Engagement.Api.Endpoints;

public static class VoteEndpoints
{
    public static void MapVoteEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/votes");

        group.MapPost("/", async (Vote vote, IVoteService service, HttpContext ctx) =>
        {
            var userId = GetUserId(ctx);
            await service.ValidateAndCastVoteAsync(vote, userId);
            return Results.Created($"/votes/{vote.Id}", vote);
        });
    }

    private static Guid GetUserId(HttpContext ctx)
    {
        // Implementar obtención de usuario autenticado
        return Guid.NewGuid(); // Placeholder
    }
}
