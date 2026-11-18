using Microsoft.AspNetCore.Mvc;

namespace LabCare.Controllers
{
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        /// <summary>
        /// Retrieves the tenant_code from the current request headers.
        /// </summary>
        protected string TenantCodeHeader
        {
            get
            {
                if (Request.Headers.TryGetValue("tenant_code", out var tenantCode))
                {
                    return tenantCode.ToString();
                }
                return string.Empty;
            }
        }

        /// <summary>
        /// Helper to check if tenant_code is present and valid.
        /// </summary>
        protected bool HasValidTenant(out string code)
        {
            if (Request.Headers.TryGetValue("tenant_code", out var tenantCode) && !string.IsNullOrEmpty(tenantCode))
            {
                code = tenantCode.ToString();
                return true;
            }
            code = string.Empty;
            return false;
        }
    }
}
