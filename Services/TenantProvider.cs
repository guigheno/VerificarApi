using System.Security.Claims;
using VerificarApi.Services.Interfaces;

namespace VerificarApi.Services
{
    // NOTA (limitação conhecida): o TenantId é gravado em cada usuário no registro,
    // mas nenhum recurso do sistema hoje filtra dados por ele. UserController (Admin)
    // gerencia todos os usuários globalmente, e Vehicles/FipeMarca/FipeModelo são
    // catálogos compartilhados sem TenantId. Ou seja, o isolamento multi-tenant da
    // meta do Ciclo 1 não está, de fato, implementado — foi mantido apenas o campo,
    // como ponto de extensão futuro caso o produto evolua para múltiplos clientes
    // isolados (hoje o modelo real é "um admin global cuidando de todos os usuários").
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