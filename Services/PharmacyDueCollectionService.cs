using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Booking.Models;
using Microsoft.Extensions.Configuration;
using SharedComponents.Rcl.Services;

namespace Booking.Services
{
    public class PharmacyDueCollectionService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _config;
        private readonly TenantSessionState _tenantState;
        private readonly JsonSerializerOptions _jsonOptions;

        public PharmacyDueCollectionService(
            IHttpClientFactory clientFactory,
            IConfiguration config,
            TenantSessionState tenantState)
        {
            _clientFactory = clientFactory;
            _config = config;
            _tenantState = tenantState;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        private static string CleanPatientName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "";
            var trimmed = name.Trim();
            var parenIdx = trimmed.IndexOf('(');
            if (parenIdx > 0)
            {
                trimmed = trimmed.Substring(0, parenIdx).Trim();
            }
            var prefixes = new[] { "Mrs.", "Mrs ", "Mr.", "Mr ", "Ms.", "Ms ", "Miss ", "Dr.", "Dr ", "Master " };
            foreach (var p in prefixes)
            {
                if (trimmed.StartsWith(p, StringComparison.OrdinalIgnoreCase))
                {
                    trimmed = trimmed.Substring(p.Length).Trim();
                    break;
                }
            }
            return trimmed;
        }

        private string GetEffectiveTenantCode(string? overrideTenant = null)
        {
            if (!string.IsNullOrWhiteSpace(overrideTenant))
            {
                return overrideTenant;
            }
            if (!string.IsNullOrWhiteSpace(_tenantState.TenantCode))
            {
                return _tenantState.TenantCode;
            }
            return "0010";
        }

        private string GetConfiguredBaseUrl()
        {
            var baseUrl = _config["ApiBaseUrl"] ?? _config["ApiSettings:MedicoApiUrl"] ?? _config["Environments:Live:ApiBaseUrl"] ?? "http://medicoapi.iscansoft.com";
            if (!baseUrl.EndsWith("/"))
            {
                baseUrl += "/";
            }
            return baseUrl;
        }

        private HttpClient CreateHttpClient(string? overrideTenant = null)
        {
            var client = _clientFactory.CreateClient("DoctorApi");
            var baseUrl = GetConfiguredBaseUrl();
            client.BaseAddress = new Uri(baseUrl);

            var tenantCode = GetEffectiveTenantCode(overrideTenant);
            
            if (client.DefaultRequestHeaders.Contains("tenantcode"))
                client.DefaultRequestHeaders.Remove("tenantcode");
            client.DefaultRequestHeaders.Add("tenantcode", tenantCode);

            if (client.DefaultRequestHeaders.Contains("tenant_code"))
                client.DefaultRequestHeaders.Remove("tenant_code");
            client.DefaultRequestHeaders.Add("tenant_code", tenantCode);

            return client;
        }

        /// <summary>
        /// 1) GET PENDING DUES
        /// GET /api/ItemMaster/getpendingdues?patientid=P1001&salestype=OP
        /// GET /api/ItemMaster/getpendingdues?mobileno=9876543210
        /// GET /api/ItemMaster/getpendingdues?patientname=Ravi&fromdate=2026-10-01&todate=2026-10-26
        /// Query params (at least one required): patientid, mobileno, salestype (OP|IP|COUNTER), patientname, billno, fromdate, todate
        /// </summary>
        public async Task<(bool Success, string Message, List<due_pending_bill> Data)> GetPendingDuesAsync(PharmacyPendingDuesFilter filter)
        {
            try
            {
                var tenantCode = GetEffectiveTenantCode(filter.tenantcode);
                var client = CreateHttpClient(tenantCode);

                async Task<(bool Success, string Message, List<due_pending_bill> Data)> ExecuteQueryAsync(List<string> qParams)
                {
                    if (qParams.Count == 0) return (false, "No criteria", new List<due_pending_bill>());
                    
                    var url = $"api/ItemMaster/getpendingdues?{string.Join("&", qParams)}";
                    var response = await client.GetAsync(url);
                    if (!response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        var fallbackClient = new HttpClient { BaseAddress = new Uri("http://medicoapitest.reachbs.com/") };
                        fallbackClient.DefaultRequestHeaders.Add("tenantcode", tenantCode);
                        fallbackClient.DefaultRequestHeaders.Add("tenant_code", tenantCode);
                        response = await fallbackClient.GetAsync(url);
                    }

                    var json = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        try
                        {
                            using var doc = JsonDocument.Parse(json);
                            if (doc.RootElement.ValueKind == JsonValueKind.Array)
                            {
                                var list = JsonSerializer.Deserialize<List<due_pending_bill>>(json, _jsonOptions);
                                if (list != null) return (true, "Success", list);
                            }
                        }
                        catch { }

                        var result = JsonSerializer.Deserialize<PharmacyPendingDuesApiResponse>(json, _jsonOptions);
                        if (result != null)
                        {
                            bool isSuccess = string.IsNullOrEmpty(result.Status) || string.Equals(result.Status, "Success", StringComparison.OrdinalIgnoreCase);
                            return (isSuccess, result.Message ?? (isSuccess ? "Success" : "Failed"), result.Data ?? new List<due_pending_bill>());
                        }
                    }

                    return (false, $"Server returned {response.StatusCode}", new List<due_pending_bill>());
                }

                List<string> BuildParams(PharmacyPendingDuesFilter f, bool includePatientName = true, bool includePatientId = true, bool includeMobile = true)
                {
                    var q = new List<string>();
                    if (includePatientId && !string.IsNullOrWhiteSpace(f.patientid))
                        q.Add($"patientid={Uri.EscapeDataString(f.patientid.Trim())}");

                    if (includeMobile && !string.IsNullOrWhiteSpace(f.mobileno))
                        q.Add($"mobileno={Uri.EscapeDataString(f.mobileno.Trim())}");

                    if (!string.IsNullOrWhiteSpace(f.salestype) && !string.Equals(f.salestype, "ALL", StringComparison.OrdinalIgnoreCase))
                        q.Add($"salestype={Uri.EscapeDataString(f.salestype.Trim().ToUpperInvariant())}");

                    if (includePatientName && !string.IsNullOrWhiteSpace(f.patientname))
                    {
                        var clean = CleanPatientName(f.patientname);
                        q.Add($"patientname={Uri.EscapeDataString(clean)}");
                    }

                    if (!string.IsNullOrWhiteSpace(f.billno))
                        q.Add($"billno={Uri.EscapeDataString(f.billno.Trim())}");

                    if (f.fromdate.HasValue)
                        q.Add($"fromdate={f.fromdate.Value:yyyy-MM-dd}");

                    if (f.todate.HasValue)
                        q.Add($"todate={f.todate.Value:yyyy-MM-dd}");

                    if (!string.IsNullOrWhiteSpace(tenantCode))
                        q.Add($"tenantcode={Uri.EscapeDataString(tenantCode)}");

                    return q;
                }

                // 1. Primary query with all provided filters
                var queryParams = BuildParams(filter, includePatientName: true, includePatientId: true, includeMobile: true);
                if (queryParams.Count == 0 || (queryParams.Count == 1 && queryParams[0].StartsWith("tenantcode=")))
                {
                    return (false, "At least one search filter is required (Patient ID, Mobile, Sales Type, Patient Name, Bill No, or Date Range).", new List<due_pending_bill>());
                }

                var primaryResult = await ExecuteQueryAsync(queryParams);
                if (primaryResult.Success && primaryResult.Data.Count > 0)
                {
                    return primaryResult;
                }

                // 2. Fallback: if patientname was provided and combined filter returned 0 bills,
                // try patientname alone (bills in pharmacy might not have patientid/mobile or might be counter bills)
                if (!string.IsNullOrWhiteSpace(filter.patientname))
                {
                    var cleanName = CleanPatientName(filter.patientname);
                    var nameOnlyParams = new List<string> { $"patientname={Uri.EscapeDataString(cleanName)}" };
                    if (!string.IsNullOrWhiteSpace(tenantCode))
                        nameOnlyParams.Add($"tenantcode={Uri.EscapeDataString(tenantCode)}");

                    var nameResult = await ExecuteQueryAsync(nameOnlyParams);
                    if (nameResult.Success && nameResult.Data.Count > 0)
                    {
                        return nameResult;
                    }

                    // Also try raw patientname if different
                    if (!string.Equals(cleanName, filter.patientname.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        var rawNameParams = new List<string> { $"patientname={Uri.EscapeDataString(filter.patientname.Trim())}" };
                        if (!string.IsNullOrWhiteSpace(tenantCode))
                            rawNameParams.Add($"tenantcode={Uri.EscapeDataString(tenantCode)}");

                        var rawResult = await ExecuteQueryAsync(rawNameParams);
                        if (rawResult.Success && rawResult.Data.Count > 0)
                        {
                            return rawResult;
                        }
                    }
                }

                // 3. Fallback: if patientid was provided and combined filter returned 0 bills, try patientid alone
                if (!string.IsNullOrWhiteSpace(filter.patientid) && queryParams.Count > 2)
                {
                    var idOnlyParams = new List<string> { $"patientid={Uri.EscapeDataString(filter.patientid.Trim())}" };
                    if (!string.IsNullOrWhiteSpace(tenantCode))
                        idOnlyParams.Add($"tenantcode={Uri.EscapeDataString(tenantCode)}");

                    var idResult = await ExecuteQueryAsync(idOnlyParams);
                    if (idResult.Success && idResult.Data.Count > 0)
                    {
                        return idResult;
                    }
                }

                return primaryResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PharmacyDueCollectionService] GetPendingDuesAsync error: {ex.Message}");
                return (false, ex.Message, new List<due_pending_bill>());
            }
        }

        /// <summary>
        /// Convenience overload for PatientId / MobileNo searches
        /// </summary>
        public Task<(bool Success, string Message, List<due_pending_bill> Data)> GetPendingDuesAsync(string? patientId, string? mobileNo)
        {
            return GetPendingDuesAsync(new PharmacyPendingDuesFilter
            {
                patientid = patientId,
                mobileno = mobileNo
            });
        }

        /// <summary>
        /// 2) INSERT DUE COLLECTION
        /// POST /api/ItemMaster/insertduecollection
        /// </summary>
        public async Task<PharmacyDueCollectionActionResponse> InsertDueCollectionAsync(due_collection_request request)
        {
            try
            {
                var client = CreateHttpClient();
                var url = "api/ItemMaster/insertduecollection";

                var response = await client.PostAsJsonAsync(url, request);
                if (!response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    var fallbackClient = new HttpClient { BaseAddress = new Uri("http://medicoapitest.reachbs.com/") };
                    fallbackClient.DefaultRequestHeaders.Add("tenantcode", GetEffectiveTenantCode());
                    response = await fallbackClient.PostAsJsonAsync(url, request);
                }

                var json = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(json))
                {
                    var resObj = JsonSerializer.Deserialize<PharmacyDueCollectionActionResponse>(json, _jsonOptions);
                    if (resObj != null) return resObj;
                }

                return new PharmacyDueCollectionActionResponse
                {
                    Status = "Failed",
                    Message = $"HTTP Error {(int)response.StatusCode}: {response.ReasonPhrase}"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PharmacyDueCollectionService] InsertDueCollectionAsync error: {ex.Message}");
                return new PharmacyDueCollectionActionResponse
                {
                    Status = "Failed",
                    Message = ex.Message
                };
            }
        }

        /// <summary>
        /// 3) BULK DUE COLLECTION (NEW)
        /// POST /api/ItemMaster/insertbulkduecollection
        /// </summary>
        public async Task<(bool Success, string Message, due_bulk_collection_result? Data)> InsertBulkDueCollectionAsync(due_bulk_collection_request request)
        {
            try
            {
                var client = CreateHttpClient();
                var url = "api/ItemMaster/insertbulkduecollection";

                var response = await client.PostAsJsonAsync(url, request);
                if (!response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    var fallbackClient = new HttpClient { BaseAddress = new Uri("http://medicoapitest.reachbs.com/") };
                    fallbackClient.DefaultRequestHeaders.Add("tenantcode", GetEffectiveTenantCode());
                    response = await fallbackClient.PostAsJsonAsync(url, request);
                }

                var json = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(json))
                {
                    var resObj = JsonSerializer.Deserialize<PharmacyBulkDueCollectionApiResponse>(json, _jsonOptions);
                    if (resObj != null)
                    {
                        return (resObj.IsSuccess, resObj.Message ?? (resObj.IsSuccess ? "Success" : "Failed"), resObj.Data);
                    }
                }

                return (false, $"HTTP Error {(int)response.StatusCode}: {response.ReasonPhrase}", null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PharmacyDueCollectionService] InsertBulkDueCollectionAsync error: {ex.Message}");
                return (false, ex.Message, null);
            }
        }

        /// <summary>
        /// 4) UPDATE DUE COLLECTION
        /// POST /api/ItemMaster/updateduecollection
        /// </summary>
        public async Task<PharmacyDueCollectionActionResponse> UpdateDueCollectionAsync(due_collection_request request)
        {
            try
            {
                var client = CreateHttpClient();
                var url = "api/ItemMaster/updateduecollection";

                var response = await client.PostAsJsonAsync(url, request);
                if (!response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    var fallbackClient = new HttpClient { BaseAddress = new Uri("http://medicoapitest.reachbs.com/") };
                    fallbackClient.DefaultRequestHeaders.Add("tenantcode", GetEffectiveTenantCode());
                    response = await fallbackClient.PostAsJsonAsync(url, request);
                }

                var json = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(json))
                {
                    var resObj = JsonSerializer.Deserialize<PharmacyDueCollectionActionResponse>(json, _jsonOptions);
                    if (resObj != null) return resObj;
                }

                return new PharmacyDueCollectionActionResponse
                {
                    Status = "Failed",
                    Message = $"HTTP Error {(int)response.StatusCode}: {response.ReasonPhrase}"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PharmacyDueCollectionService] UpdateDueCollectionAsync error: {ex.Message}");
                return new PharmacyDueCollectionActionResponse
                {
                    Status = "Failed",
                    Message = ex.Message
                };
            }
        }

        /// <summary>
        /// 5) DELETE DUE COLLECTION (soft delete, restores bill balance)
        /// GET /api/ItemMaster/deleteduecollection?duecollectioncode=1
        /// </summary>
        public async Task<PharmacyDueCollectionActionResponse> DeleteDueCollectionAsync(long dueCollectionCode)
        {
            try
            {
                var client = CreateHttpClient();
                var url = $"api/ItemMaster/deleteduecollection?duecollectioncode={dueCollectionCode}";

                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    var fallbackClient = new HttpClient { BaseAddress = new Uri("http://medicoapitest.reachbs.com/") };
                    fallbackClient.DefaultRequestHeaders.Add("tenantcode", GetEffectiveTenantCode());
                    response = await fallbackClient.GetAsync(url);
                }

                var json = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(json))
                {
                    var resObj = JsonSerializer.Deserialize<PharmacyDueCollectionActionResponse>(json, _jsonOptions);
                    if (resObj != null) return resObj;
                }

                return new PharmacyDueCollectionActionResponse
                {
                    Status = "Failed",
                    Message = $"HTTP Error {(int)response.StatusCode}: {response.ReasonPhrase}"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PharmacyDueCollectionService] DeleteDueCollectionAsync error: {ex.Message}");
                return new PharmacyDueCollectionActionResponse
                {
                    Status = "Failed",
                    Message = ex.Message
                };
            }
        }

        /// <summary>
        /// 6) GET ALL DUE COLLECTIONS
        /// GET /api/ItemMaster/getallduecollections
        /// </summary>
        public async Task<(bool Success, string Message, List<PharmacyDueCollectionRecord> Data)> GetAllDueCollectionsAsync()
        {
            try
            {
                var client = CreateHttpClient();
                var url = "api/ItemMaster/getallduecollections";

                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    var fallbackClient = new HttpClient { BaseAddress = new Uri("http://medicoapitest.reachbs.com/") };
                    fallbackClient.DefaultRequestHeaders.Add("tenantcode", GetEffectiveTenantCode());
                    response = await fallbackClient.GetAsync(url);
                }

                var json = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(json))
                {
                    var resObj = JsonSerializer.Deserialize<PharmacyAllDueCollectionsApiResponse>(json, _jsonOptions);
                    if (resObj != null)
                    {
                        bool isSuccess = string.Equals(resObj.Status, "Success", StringComparison.OrdinalIgnoreCase);
                        return (isSuccess, resObj.Message ?? "Success", resObj.Data ?? new List<PharmacyDueCollectionRecord>());
                    }
                }

                return (false, $"HTTP Error {(int)response.StatusCode}", new List<PharmacyDueCollectionRecord>());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PharmacyDueCollectionService] GetAllDueCollectionsAsync error: {ex.Message}");
                return (false, ex.Message, new List<PharmacyDueCollectionRecord>());
            }
        }

        /// <summary>
        /// 7) GET DUE COLLECTION BY CODE
        /// GET /api/ItemMaster/getduecollectionbycode?duecollectioncode=1
        /// </summary>
        public async Task<PharmacyDueCollectionRecord?> GetDueCollectionByCodeAsync(long dueCollectionCode)
        {
            try
            {
                var client = CreateHttpClient();
                var url = $"api/ItemMaster/getduecollectionbycode?duecollectioncode={dueCollectionCode}";

                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode && response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    var fallbackClient = new HttpClient { BaseAddress = new Uri("http://medicoapitest.reachbs.com/") };
                    fallbackClient.DefaultRequestHeaders.Add("tenantcode", GetEffectiveTenantCode());
                    response = await fallbackClient.GetAsync(url);
                }

                var json = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(json))
                {
                    var resObj = JsonSerializer.Deserialize<PharmacyDueCollectionByCodeApiResponse>(json, _jsonOptions);
                    return resObj?.Data;
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PharmacyDueCollectionService] GetDueCollectionByCodeAsync error: {ex.Message}");
                return null;
            }
        }
    }
}
