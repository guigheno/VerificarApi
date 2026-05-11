using System.Security.Claims;
using VerificarApi.Services.Interfaces;

namespace VerificarApi.Services
{
    public class TenantProvider : ITenantProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid GetTenantId()
        {
            var tenantIdClaim = _httpContextAccessor.HttpContext?
                .User?
                .FindFirst("tenantId")?
                .Value;

            if (string.IsNullOrWhiteSpace(tenantIdClaim))
                throw new Exception("Tenant não encontrado no token.");

            return Guid.Parse(tenantIdClaim);
        }
    }
}