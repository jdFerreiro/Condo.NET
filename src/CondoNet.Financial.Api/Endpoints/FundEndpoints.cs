using CondoNet.Financial.Core.Interfaces;

namespace CondoNet.Financial.Api.Endpoints;

public static class FundEndpoints
{
    public static void MapFundEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/finance/funds/{orgId}", async (Guid orgId, IGlobalFundRepository repo) =>
        {
            var funds = await repo.GetByOrganizationIdAsync(orgId);
            return Results.Ok(funds);
        });
    }
}
