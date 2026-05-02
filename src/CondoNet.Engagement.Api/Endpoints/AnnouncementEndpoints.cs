using CondoNet.Engagement.Core.Entities;
using CondoNet.Engagement.Core.Services;

namespace CondoNet.Engagement.Api.Endpoints;

public static class AnnouncementEndpoints
{
    public static void MapAnnouncementEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/announcements");

        group.MapPost("/", async (Announcement announcement, IAnnouncementService service, HttpContext ctx) =>
        {
            var userId = GetUserId(ctx);
            await service.ValidateAndCreateAsync(announcement, userId);
            return Results.Created($"/announcements/{announcement.Id}", announcement);
        });

        group.MapPut("/{id}", async (Guid id, Announcement announcement, IAnnouncementService service, HttpContext ctx) =>
        {
            var userId = GetUserId(ctx);
            announcement.Id = id;
            await service.ValidateAndUpdateAsync(announcement, userId);
            return Results.NoContent();
        });

        group.MapDelete("/{id}", async (Guid id, IAnnouncementService service, HttpContext ctx) =>
        {
            var userId = GetUserId(ctx);
            await service.ValidateAndDeleteAsync(id, userId);
            return Results.NoContent();
        });
    }

    private static Guid GetUserId(HttpContext ctx)
    {
        ArgumentNullException.ThrowIfNull(ctx);
        // Implementar obtención de usuario autenticado
        return Guid.NewGuid(); // Placeholder
    }
}
