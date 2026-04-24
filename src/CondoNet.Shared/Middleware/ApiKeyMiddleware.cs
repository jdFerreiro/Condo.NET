using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace CondoNet.Shared.Middleware
{
    public class ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        private readonly RequestDelegate _next = next;
        private readonly string _authServiceUrl = configuration["AuthService:Url"] ?? "";
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Headers.TryGetValue("X-Api-Key", out var extractedApiKey) || string.IsNullOrEmpty(extractedApiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("ApiKey faltante.");
                return;
            }

            if (context.Request.Path.Value.Contains("apikeys/validate"))
            {
                context.Response.StatusCode = 200;
                return;
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("x-api-key", [.. extractedApiKey]);
            var response = await client.GetAsync($"{_authServiceUrl}/api/auth/apikeys/validate?key={extractedApiKey}");

            if (!response.IsSuccessStatusCode)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("ApiKey inválida.");
                return;
            }

            await _next(context);
        }
    }

    public static class ApiKeyMiddlewareExtensions
    {
        public static IApplicationBuilder UseApiKeyMiddleware(this IApplicationBuilder builder)
            => builder.UseMiddleware<ApiKeyMiddleware>();
    }
}