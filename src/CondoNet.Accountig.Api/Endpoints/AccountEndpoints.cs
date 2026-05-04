using CondoNet.Accounting.Core.Entities;
using CondoNet.Accounting.Core.Repositories;
using CondoNet.Accounting.Core.Services;

namespace CondoNet.Accounting.Api.Endpoints;

public static class AccountEndpoints
{
    public static void MapAccountEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/accounts");

        group.MapGet("/", async (Guid organizationId, Guid condominiumId, IAccountRepository repo) =>
        {
            var accounts = await repo.GetAllAsync(organizationId, condominiumId);
            return Results.Ok(accounts);
        });

        group.MapGet("/{id}", async (Guid id, IAccountRepository repo) =>
        {
            var account = await repo.GetByIdAsync(id);
            return account is not null ? Results.Ok(account) : Results.NotFound();
        });

        group.MapPost("/", async (Account account, IAccountRepository repo) =>
        {
            await repo.AddAsync(account);
            return Results.Created($"/accounts/{account.Id}", account);
        });

        group.MapPut("/{id}", async (Guid id, Account account, IAccountRepository repo) =>
        {
            account.Id = id;
            await repo.UpdateAsync(account);
            return Results.NoContent();
        });

        group.MapDelete("/{id}", async (Guid id, IAccountRepository repo) =>
        {
            await repo.DeleteAsync(id);
            return Results.NoContent();
        });
    }
}
