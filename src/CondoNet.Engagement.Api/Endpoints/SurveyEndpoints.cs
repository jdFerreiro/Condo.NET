using CondoNet.Engagement.Core.Entities;
using CondoNet.Engagement.Core.Services;

namespace CondoNet.Engagement.Api.Endpoints;

public static class SurveyEndpoints
{
    public static void MapSurveyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/surveys");

        group.MapPost("/", async (Survey survey, ISurveyService service, HttpContext ctx) =>
        {
            var userId = GetUserId(ctx);
            await service.ValidateAndCreateAsync(survey, userId);
            return Results.Created($"/surveys/{survey.Id}", survey);
        });

        group.MapPost("/{id}/respond", async (Guid id, object response, ISurveyService service, HttpContext ctx) =>
        {
            var userId = GetUserId(ctx);
            await service.ValidateAndRespondAsync(id, userId, response);
            return Results.NoContent();
        });
    }

    private static Guid GetUserId(HttpContext ctx)
    {
        // Implementar obtención de usuario autenticado
        return Guid.NewGuid(); // Placeholder
    }
}
