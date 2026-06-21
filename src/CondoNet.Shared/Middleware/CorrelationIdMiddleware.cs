using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace CondoNet.Shared.Middleware;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeaderKey = "X-Correlation-ID";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 1. Intentar obtener el ID del Header, si no existe, generar uno nuevo
        if (!context.Request.Headers.TryGetValue(CorrelationIdHeaderKey, out var correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }

        // 2. Devolver el ID en la respuesta para facilitar el rastreo (opcional pero recomendado)
        context.Response.Headers.Append(CorrelationIdHeaderKey, correlationId);

        // 3. Introducir el ID en el contexto de Serilog. 
        // ¡Ojo! Debe coincidir exactamente con el nombre de tu plantilla: {CorrelationId}
        using (LogContext.PushProperty("CorrelationId", correlationId.ToString()))
        {
            await _next(context);
        }
    }
}
