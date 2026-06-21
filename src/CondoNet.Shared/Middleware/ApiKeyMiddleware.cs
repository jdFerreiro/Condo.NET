using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Security.Claims;

namespace CondoNet.Shared.Middleware
{
    public static class ApiKeyMiddlewareExtensions
    {
        public static IApplicationBuilder UseApiKeyMiddleware(this IApplicationBuilder builder)
            => builder.UseMiddleware<ApiKeyMiddleware>();
    }
    public class ApiKeyMiddleware(RequestDelegate next, IHttpClientFactory httpClientFactory, ILogger<ApiKeyMiddleware> logger)
    {
        private readonly RequestDelegate _next = next;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly ILogger<ApiKeyMiddleware> _logger = logger;
        private const string ApiKeyHeaderKey = "X-Api-Key";

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                var path = context.Request.Path.Value ?? string.Empty;

                // 1. Permitir acceso libre a Swagger, OpenAPI y endpoints de salud pública
                if (path.Contains("swagger", StringComparison.OrdinalIgnoreCase) || path.Contains("health", StringComparison.OrdinalIgnoreCase))
                {
                    await _next(context);
                    return;
                }

                // 2. Validar presencia de la ApiKey
                if (!context.Request.Headers.TryGetValue(ApiKeyHeaderKey, out var extractedApiKey) || string.IsNullOrEmpty(extractedApiKey))
                {
                    _logger.LogWarning("Intento de acceso rechazado: Falta el header {Header}", ApiKeyHeaderKey);
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("ApiKey faltante.");
                    return;
                }

                // Evitar bucle infinito si el mismo servicio de autenticación es el que valida
                if (path.Contains("apikeys/validate", StringComparison.OrdinalIgnoreCase))
                {
                    await _next(context);
                    return;
                }

                // 3. Consumir el HttpClient nombrado configurado en Program.cs
                // ¡IMPORTANTE! No limpies los headers, para que viaje el CorrelationId de forma nativa
                var client = _httpClientFactory.CreateClient("AuthService");

                // Añadimos la llave específica de validación a los headers de esta petición puntual
                client.DefaultRequestHeaders.Add(ApiKeyHeaderKey, extractedApiKey.ToString());

                // Usamos una ruta relativa porque la BaseAddress ya está resuelta por el Factory
                string relativeUrl = $"api/auth/apikeys/validate?key={extractedApiKey}";
                var response = await client.GetAsync(relativeUrl);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("ApiKey inválida o rechazada por el servicio de autenticación.");
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("ApiKey inválida.");
                    return;
                }

                var organizationId = await response.Content.ReadFromJsonAsync<Guid>();
                var identity = new ClaimsIdentity([new Claim("OrganizationId", organizationId.ToString())], "ApiKeyAuth");
                context.User.AddIdentity(identity); // El TenantsService podrá extraer esta claim para configurar el contexto multi-tenant

                await _next(context);
            }
            catch (Exception ex)
            {
                // Centralizamos el error en Serilog usando el formato JSON estruturado
                _logger.LogError(ex, "Error crítico durante la validación de la ApiKey en el middleware.");

                context.Response.StatusCode = 500;
                context.Response.ContentType = "text/plain";

                // Mensaje seguro para el cliente evitando caídas por InnerException nulo
                var innerMessage = ex.InnerException != null ? ex.InnerException.Message : "Ninguno";
                await context.Response.WriteAsync($"Error interno: {ex.Message} \n Internal: {innerMessage}");
            }
        }

        //public async Task InvokeAsync(HttpContext context)
        //{
        //    try
        //    {
        //        var path = context.Request.Path.Value;

        //        // Permite el acceso sin ApiKey a Swagger y OpenAPI
        //        if (!path.Contains("swagger", StringComparison.OrdinalIgnoreCase))
        //        {

        //            if (!context.Request.Headers.TryGetValue("X-Api-Key", out var extractedApiKey) || string.IsNullOrEmpty(extractedApiKey))
        //            {
        //                context.Response.StatusCode = 401;
        //                await context.Response.WriteAsync("ApiKey faltante.");
        //                return;
        //            }

        //            if (context.Request.Path.Value.Contains("apikeys/validate"))
        //            {
        //                context.Response.StatusCode = 200;
        //                return;
        //            }

        //            var client = _httpClientFactory.CreateClient("AuthService");
        //            client.BaseAddress = new Uri(_authServiceUrl);
        //            client.DefaultRequestHeaders.Clear();
        //            client.DefaultRequestHeaders.Add("x-api-key", [.. extractedApiKey]);
        //            string url = $"{_authServiceUrl}/api/auth/apikeys/validate?key={extractedApiKey}";
        //            var response = await client.GetAsync(url);

        //            if (!response.IsSuccessStatusCode)
        //            {
        //                context.Response.StatusCode = 401;
        //                await context.Response.WriteAsync("ApiKey inválida.");
        //                return;
        //            }
        //        }

        //        await _next(context);
        //    }
        //    catch (Exception ex)
        //    {
        //        context.Response.StatusCode = 500;
        //        await context.Response.WriteAsync($"Error interno: {ex.Message} /n Internal: {ex.InnerException!.Message} /n StackTrace: {ex.StackTrace}");
        //    }
        //}
    }
}