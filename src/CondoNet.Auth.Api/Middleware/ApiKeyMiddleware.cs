using CondoNet.Auth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Auth.Api.Middleware
{
    public class ApiKeyMiddleware(RequestDelegate next)
    {
        private const string ApiKeyHeaderName = "X-Api-Key";

        public async Task InvokeAsync(HttpContext context, AuthDbContext dbContext)
        {
            if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("API Key faltante.");
                return;
            }

            // Validación contra la base de datos
            var isValid = await dbContext.ApiKeys
                .AnyAsync(x => x.Key == extractedApiKey.ToString() && x.IsActive);

            if (!isValid)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("API Key inválida o inactiva.");
                return;
            }

            await next(context);
        }
    }
}