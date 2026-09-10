using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using medico_backend.Model;

namespace Booking.Services
{
    public class NurseNotesMasterService
    {
        private readonly HttpClient _http;
        private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public NurseNotesMasterService(HttpClient http)
        {
            _http = http;
        }

        private void AddTenantHeaders(HttpRequestMessage req, string? tenantCode)
        {
            if (!string.IsNullOrWhiteSpace(tenantCode))
            {
                req.Headers.Remove("tenant_code");
                req.Headers.Remove("tenantcode");
                req.Headers.Remove("tenant-code");
                req.Headers.Remove("TenantId");
                req.Headers.Add("tenant_code", tenantCode);
                req.Headers.Add("tenantcode", tenantCode);
                req.Headers.Add("tenant-code", tenantCode);
                req.Headers.Add("TenantId", tenantCode);
            }
        }

        private string AppendTenantQuery(string url, string? tenantCode)
        {
            if (string.IsNullOrWhiteSpace(tenantCode)) return url;
            var sep = url.Contains("?") ? "&" : "?";
            return $"{url}{sep}tenant_code={tenantCode}&tenantCode={tenantCode}&TenantId={tenantCode}";
        }

        private List<T> DeserializeList<T>(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return new();
            try
            {
                using var doc = JsonDocument.Parse(raw);
                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    return JsonSerializer.Deserialize<List<T>>(raw, _jsonOptions) ?? new();
                }
                if (doc.RootElement.TryGetProperty("value", out var val) && val.ValueKind == JsonValueKind.Array)
                {
                    return JsonSerializer.Deserialize<List<T>>(val.GetRawText(), _jsonOptions) ?? new();
                }
                if (doc.RootElement.TryGetProperty("data", out var dataVal) && dataVal.ValueKind == JsonValueKind.Array)
                {
                    return JsonSerializer.Deserialize<List<T>>(dataVal.GetRawText(), _jsonOptions) ?? new();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NurseNotesMasterService] DeserializeList error for {typeof(T).Name}: {ex.Message}");
            }
            return new();
        }

        // ═══════════════════════════════════════
        // 1. IO Particulars Master
        // ═══════════════════════════════════════
        public async Task<List<IoParticularsMasterModel>> GetIoParticularsAsync(bool activeOnly = false, string? tenantCode = null)
        {
            try
            {
                var url = AppendTenantQuery($"api/MasterList/io-particulars?activeOnly={activeOnly.ToString().ToLower()}", tenantCode);
                using var req = new HttpRequestMessage(HttpMethod.Get, url);
                AddTenantHeaders(req, tenantCode);

                var response = await _http.SendAsync(req);
                if (!response.IsSuccessStatusCode) return new List<IoParticularsMasterModel>();

                var raw = await response.Content.ReadAsStringAsync();
                return DeserializeList<IoParticularsMasterModel>(raw);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetIoParticularsAsync] Error: {ex.Message}");
                return new List<IoParticularsMasterModel>();
            }
        }

        public async Task<bool> AddIoParticularAsync(AddIoParticularRequest request, string? tenantCode = null)
        {
            try
            {
                var effectiveTenant = !string.IsNullOrWhiteSpace(tenantCode) ? tenantCode : request.tenant_code;
                if (string.IsNullOrWhiteSpace(request.tenant_code) && !string.IsNullOrWhiteSpace(effectiveTenant))
                {
                    request.tenant_code = effectiveTenant;
                }

                var url = AppendTenantQuery("api/MasterList/io-particulars/add", effectiveTenant);
                using var req = new HttpRequestMessage(HttpMethod.Post, url);
                req.Content = JsonContent.Create(request);
                AddTenantHeaders(req, effectiveTenant);

                var response = await _http.SendAsync(req);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AddIoParticularAsync] Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateIoParticularAsync(UpdateIoParticularRequest request, string? tenantCode = null)
        {
            try
            {
                var effectiveTenant = !string.IsNullOrWhiteSpace(tenantCode) ? tenantCode : request.tenant_code;
                if (string.IsNullOrWhiteSpace(request.tenant_code) && !string.IsNullOrWhiteSpace(effectiveTenant))
                {
                    request.tenant_code = effectiveTenant;
                }

                var url = AppendTenantQuery("api/MasterList/io-particulars/update", effectiveTenant);
                using var req = new HttpRequestMessage(HttpMethod.Post, url);
                req.Content = JsonContent.Create(request);
                AddTenantHeaders(req, effectiveTenant);

                var response = await _http.SendAsync(req);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UpdateIoParticularAsync] Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteIoParticularAsync(int particularId, string? tenantCode = null)
        {
            try
            {
                var url = AppendTenantQuery($"api/MasterList/io-particulars/delete?particular_id={particularId}", tenantCode);
                using var req = new HttpRequestMessage(HttpMethod.Get, url);
                AddTenantHeaders(req, tenantCode);

                var response = await _http.SendAsync(req);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DeleteIoParticularAsync] Error: {ex.Message}");
                return false;
            }
        }

        // ═══════════════════════════════════════
        // 2. Service Name Master
        // ═══════════════════════════════════════
        public async Task<List<ServiceNameMasterModel>> GetServiceNamesAsync(bool activeOnly = false, string? tenantCode = null)
        {
            try
            {
                var url = AppendTenantQuery($"api/MasterList/service-name?activeOnly={activeOnly.ToString().ToLower()}", tenantCode);
                using var req = new HttpRequestMessage(HttpMethod.Get, url);
                AddTenantHeaders(req, tenantCode);

                var response = await _http.SendAsync(req);
                if (!response.IsSuccessStatusCode) return new List<ServiceNameMasterModel>();

                var raw = await response.Content.ReadAsStringAsync();
                return DeserializeList<ServiceNameMasterModel>(raw);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetServiceNamesAsync] Error: {ex.Message}");
                return new List<ServiceNameMasterModel>();
            }
        }

        public async Task<bool> AddServiceNameAsync(AddServiceNameRequest request, string? tenantCode = null)
        {
            try
            {
                var effectiveTenant = !string.IsNullOrWhiteSpace(tenantCode) ? tenantCode : request.tenant_code;
                if (string.IsNullOrWhiteSpace(request.tenant_code) && !string.IsNullOrWhiteSpace(effectiveTenant))
                {
                    request.tenant_code = effectiveTenant;
                }

                var url = AppendTenantQuery("api/MasterList/service-name/add", effectiveTenant);
                using var req = new HttpRequestMessage(HttpMethod.Post, url);
                req.Content = JsonContent.Create(request);
                AddTenantHeaders(req, effectiveTenant);

                var response = await _http.SendAsync(req);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AddServiceNameAsync] Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateServiceNameAsync(UpdateServiceNameRequest request, string? tenantCode = null)
        {
            try
            {
                var effectiveTenant = !string.IsNullOrWhiteSpace(tenantCode) ? tenantCode : request.tenant_code;
                if (string.IsNullOrWhiteSpace(request.tenant_code) && !string.IsNullOrWhiteSpace(effectiveTenant))
                {
                    request.tenant_code = effectiveTenant;
                }

                var url = AppendTenantQuery("api/MasterList/service-name/update", effectiveTenant);
                using var req = new HttpRequestMessage(HttpMethod.Post, url);
                req.Content = JsonContent.Create(request);
                AddTenantHeaders(req, effectiveTenant);

                var response = await _http.SendAsync(req);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UpdateServiceNameAsync] Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteServiceNameAsync(int serviceId, string? tenantCode = null)
        {
            try
            {
                var url = AppendTenantQuery($"api/MasterList/service-name/delete?service_id={serviceId}", tenantCode);
                using var req = new HttpRequestMessage(HttpMethod.Get, url);
                AddTenantHeaders(req, tenantCode);

                var response = await _http.SendAsync(req);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DeleteServiceNameAsync] Error: {ex.Message}");
                return false;
            }
        }

        // ═══════════════════════════════════════
        // 3. Schedule Type Master
        // ═══════════════════════════════════════
        public async Task<List<ScheduleTypeMasterModel>> GetScheduleTypesAsync(bool activeOnly = false, string? tenantCode = null)
        {
            try
            {
                var url = AppendTenantQuery($"api/MasterList/schedule-type?activeOnly={activeOnly.ToString().ToLower()}", tenantCode);
                using var req = new HttpRequestMessage(HttpMethod.Get, url);
                AddTenantHeaders(req, tenantCode);

                var response = await _http.SendAsync(req);
                if (!response.IsSuccessStatusCode) return new List<ScheduleTypeMasterModel>();

                var raw = await response.Content.ReadAsStringAsync();
                return DeserializeList<ScheduleTypeMasterModel>(raw);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetScheduleTypesAsync] Error: {ex.Message}");
                return new List<ScheduleTypeMasterModel>();
            }
        }

        public async Task<bool> AddScheduleTypeAsync(AddScheduleTypeRequest request, string? tenantCode = null)
        {
            try
            {
                var effectiveTenant = !string.IsNullOrWhiteSpace(tenantCode) ? tenantCode : request.tenant_code;
                if (string.IsNullOrWhiteSpace(request.tenant_code) && !string.IsNullOrWhiteSpace(effectiveTenant))
                {
                    request.tenant_code = effectiveTenant;
                }

                var url = AppendTenantQuery("api/MasterList/schedule-type/add", effectiveTenant);
                using var req = new HttpRequestMessage(HttpMethod.Post, url);
                req.Content = JsonContent.Create(request);
                AddTenantHeaders(req, effectiveTenant);

                var response = await _http.SendAsync(req);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AddScheduleTypeAsync] Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateScheduleTypeAsync(UpdateScheduleTypeRequest request, string? tenantCode = null)
        {
            try
            {
                var effectiveTenant = !string.IsNullOrWhiteSpace(tenantCode) ? tenantCode : request.tenant_code;
                if (string.IsNullOrWhiteSpace(request.tenant_code) && !string.IsNullOrWhiteSpace(effectiveTenant))
                {
                    request.tenant_code = effectiveTenant;
                }

                var url = AppendTenantQuery("api/MasterList/schedule-type/update", effectiveTenant);
                using var req = new HttpRequestMessage(HttpMethod.Post, url);
                req.Content = JsonContent.Create(request);
                AddTenantHeaders(req, effectiveTenant);

                var response = await _http.SendAsync(req);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UpdateScheduleTypeAsync] Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteScheduleTypeAsync(int typeId, string? tenantCode = null)
        {
            try
            {
                var url = AppendTenantQuery($"api/MasterList/schedule-type/delete?type_id={typeId}", tenantCode);
                using var req = new HttpRequestMessage(HttpMethod.Get, url);
                AddTenantHeaders(req, tenantCode);

                var response = await _http.SendAsync(req);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DeleteScheduleTypeAsync] Error: {ex.Message}");
                return false;
            }
        }
    }
}
