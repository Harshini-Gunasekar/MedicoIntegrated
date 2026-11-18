using LabCare.Services;

namespace LabCare.Handlers;

public class TenantHeaderHandler : DelegatingHandler
{
    private readonly TenantSessionState _session;

    public TenantHeaderHandler(TenantSessionState session)
    {
        _session = session;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // 1. Inject Bearer token if not already present
        if (!string.IsNullOrEmpty(_session.AuthToken) && request.Headers.Authorization == null)
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.AuthToken);
        }

        // 2. Inject Tenant Headers if not already present
        if (!string.IsNullOrEmpty(_session.TenantCode))
        {
            if (!request.Headers.Contains("tenant_code"))
                request.Headers.Add("tenant_code", _session.TenantCode);
            
            if (!request.Headers.Contains("X-Tenant-Code"))
                request.Headers.Add("X-Tenant-Code", _session.TenantCode);
        }

        if (_session.UserCode > 0 && !request.Headers.Contains("X-User-Code"))
        {
            request.Headers.Add("X-User-Code", _session.UserCode.ToString());
        }

        // 3. Inject Branch and Counter if not already present
        if (_session.BranchCode > 0 && !request.Headers.Contains("bhcode"))
        {
            request.Headers.Add("bhcode", _session.BranchCode.ToString());
        }

        if (_session.CounterCode > 0 && !request.Headers.Contains("cntcode"))
        {
            request.Headers.Add("cntcode", _session.CounterCode.ToString());
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
