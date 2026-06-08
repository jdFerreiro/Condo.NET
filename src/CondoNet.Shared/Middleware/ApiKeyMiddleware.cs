using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace CondoNet.Shared.Middleware
{
    public static class ApiKeyMiddlewareExtensions
    {
        public static IApplicationBuilder UseApiKeyMiddleware(this IApplicationBuilder builder)
            => builder.UseMiddleware<ApiKeyMiddleware>();
    }
    public class ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        private readonly RequestDelegate _next = next;
        private readonly string _authServiceUrl = configuration["AuthService:Url"] ?? "";
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                var path = context.Request.Path.Value;

                // Permite el acceso sin ApiKey a Swagger y OpenAPI
                if (!path.Contains("swagger", StringComparison.OrdinalIgnoreCase))
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

                    var client = _httpClientFactory.CreateClient("AuthService");
                    client.BaseAddress = new Uri(_authServiceUrl);
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Add("x-api-key", [.. extractedApiKey]);
                    string url = $"{_authServiceUrl}/api/auth/apikeys/validate?key={extractedApiKey}";
                    var response = await client.GetAsync(url);

                    if (!response.IsSuccessStatusCode)
                    {
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsync("ApiKey inválida.");
                        return;
                    }
                }

                await _next(context);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync($"Error interno: {ex.Message} /n Internal: {ex.InnerException!.Message} /n StackTrace: {ex.StackTrace}");
            }
        }
    }
}