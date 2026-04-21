using CondoNet.Asset.Core.Interfaces;
using Microsoft.AspNetCore.Http;

namespace CondoNet.Asset.Infraestructure.Services
{
    public class TenantService(IHttpContextAccessor httpContextAccessor) : ITenantService
    {
        public Guid GetOrganizationId()
        {
            // Se busca el claim 'org_id' definido en el esquema de seguridad (Pág. 14 del PDF)
            var claim = httpContextAccessor.HttpContext?.User?.FindFirst("org_id")?.Value;

            if (string.IsNullOrEmpty(claim))
            {
                // En un microservicio, si no hay org_id en el token, la petición no es válida
                throw new UnauthorizedAccessException("El token JWT no contiene una organización válida.");
            }

            return Guid.Parse(claim);
        }
    }
}
