using CondoNet.Engagement.Core.Entities;
using CondoNet.Engagement.Core.Services;

namespace CondoNet.Engagement.Api.Endpoints;

public static class NotificationEndpoints
{
    public static void MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/notifications");

        group.MapPost("/", async (Notification notification, INotificationService service, HttpContext ctx) =>
        {
            var userId = GetUserId(ctx);
            await service.ValidateAndSendAsync(notification, userId);
            return Results.Created($"/notifications/{notification.Id}", notification);
        });

        group.MapPut("/{id}/read", async (Guid id, INotificationService service, HttpContext ctx) =>
        {
            var userId = GetUserId(ctx);
            await service.MarkAsReadAsync(id, userId);
            return Results.NoContent();
        });
    }

    private static Guid GetUserId(HttpContext ctx)
    {
        // Implementar obtención de usuario autenticado
        return Guid.NewGuid(); // Placeholder
    }
}
