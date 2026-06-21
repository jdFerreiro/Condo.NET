using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CondoNet.Shared.Handlers
{
    public class InternalHttpGatewayHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<InternalHttpGatewayHandler> _logger;
        private const string CorrelationIdHeaderKey = "X-Correlation-ID";

        public InternalHttpGatewayHandler(IHttpContextAccessor httpContextAccessor, ILogger<InternalHttpGatewayHandler> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // 1. Extraer el CorrelationId de la petición HTTP actual del usuario
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null && httpContext.Request.Headers.TryGetValue(CorrelationIdHeaderKey, out var correlationId))
            {
                // 2. Propagarlo automáticamente a la llamada interna del microservicio
                request.Headers.TryAddWithoutValidation(CorrelationIdHeaderKey, correlationId.ToString());
            }

            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Iniciando llamada HTTP interna: {Method} {Url}", request.Method, request.RequestUri);

            try
            {
                var response = await base.SendAsync(request, cancellationToken);
                stopwatch.Stop();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Llamada HTTP interna fallida: {Method} {Url} - Status: {StatusCode} (Tiempo: {Elapsed}ms)",
                        request.Method, request.RequestUri, response.StatusCode, stopwatch.ElapsedMilliseconds);
                }
                else
                {
                    _logger.LogInformation("Llamada HTTP interna exitosa: {Method} {Url} - Status: {StatusCode} (Tiempo: {Elapsed}ms)",
                        request.Method, request.RequestUri, response.StatusCode, stopwatch.ElapsedMilliseconds);
                }

                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error crítico de red en llamada HTTP interna: {Method} {Url} (Tiempo: {Elapsed}ms)",
                    request.Method, request.RequestUri, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }
    }
}
