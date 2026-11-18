using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Text;
using System.Net.Http.Json;

public class TenantHttpService
{
    private readonly HttpClient _http;
    private readonly TenantState _tenantState;

    public TenantHttpService(HttpClient http, TenantState tenantState)
    {
        _http = http;
        _tenantState = tenantState;
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string url, string token = null)
    {
        var request = new HttpRequestMessage(method, url);

        if (!string.IsNullOrWhiteSpace(_tenantState.TenantID))
            request.Headers.Add("TenantID", _tenantState.TenantID);

        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return request;
    }

    public async Task<HttpResponseMessage> GetAsync(string url, string token = null)
    {
        var request = CreateRequest(HttpMethod.Get, url, token);
        return await _http.SendAsync(request);
    }

    public async Task<HttpResponseMessage> DeleteAsync(string url)
    {
        var request = CreateRequest(HttpMethod.Delete, url);
        return await _http.SendAsync(request);
    }

    public async Task<HttpResponseMessage> PostAsync<T>(string url, T data)
    {
        var request = CreateRequest(HttpMethod.Post, url);
        request.Content = new StringContent(
            JsonConvert.SerializeObject(data),
            Encoding.UTF8,
            "application/json");

        return await _http.SendAsync(request);
    }

    public async Task<HttpResponseMessage> PutAsync<T>(string url, T data)
    {
        var request = CreateRequest(HttpMethod.Put, url);
        request.Content = new StringContent(
            JsonConvert.SerializeObject(data),
            Encoding.UTF8,
            "application/json");

        return await _http.SendAsync(request);
    }
}