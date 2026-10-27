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

        private string GetEffectiveTenantCode()
        {
            if (!string.IsNullOrWhiteSpace(_tenantState.TenantCode))
            {
                return _tenantState.TenantCode;
            }
            return "TEN001";
        }

        private string GetConfiguredBaseUrl()
        {
            var baseUrl = _config["ApiBaseUrl"] ?? _config["ApiSettings:MedicoApiUrl"] ?? "http://medicoapi.iscansoft.com";
            if (!baseUrl.EndsWith("/"))
            {
                baseUrl += "/";
            }
            return baseUrl;
        }

        private HttpClient CreateHttpClient()
        {
            var client = _clientFactory.CreateClient("DoctorApi");
            var tenantCode = GetEffectiveTenantCode();
            
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
        /// GET /api/ItemMaster/getpendingdues?patientid=P1001 or mobileno=9876543210
        /// </summary>
        public async Task<(bool Success, string Message, List<PharmacyPendingDueBill> Data)> GetPendingDuesAsync(string? patientId, string? mobileNo)
        {
            try
            {
                var queryParams = new List<string>();
                if (!string.IsNullOrWhiteSpace(patientId))
                    queryParams.Add($"patientid={Uri.EscapeDataString(patientId.Trim())}");
                if (!string.IsNullOrWhiteSpace(mobileNo))
                    queryParams.Add($"mobileno={Uri.EscapeDataString(mobileNo.Trim())}");

                if (queryParams.Count == 0)
                {
                    return (false, "patientid or mobileno is required.", new List<PharmacyPendingDueBill>());
                }

                var url = $"api/ItemMaster/getpendingdues?{string.Join("&", queryParams)}";
                var client = CreateHttpClient();

                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode && (response.StatusCode == System.Net.HttpStatusCode.NotFound))
                {
                    // Fallback attempt against test API if live returns 404
                    var fallbackClient = new HttpClient { BaseAddress = new Uri("http://medicoapitest.reachbs.com/") };
                    fallbackClient.DefaultRequestHeaders.Add("tenantcode", GetEffectiveTenantCode());
                    response = await fallbackClient.GetAsync(url);
                }

                var json = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(json))
                {
                    var result = JsonSerializer.Deserialize<PharmacyPendingDuesApiResponse>(json, _jsonOptions);
                    if (result != null)
                    {
                        bool isSuccess = string.Equals(result.Status, "Success", StringComparison.OrdinalIgnoreCase);
                        return (isSuccess, result.Message ?? (isSuccess ? "Success" : "Failed"), result.Data ?? new List<PharmacyPendingDueBill>());
                    }
                }

                return (false, $"Server returned {response.StatusCode}", new List<PharmacyPendingDueBill>());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PharmacyDueCollectionService] GetPendingDuesAsync error: {ex.Message}");
                return (false, ex.Message, new List<PharmacyPendingDueBill>());
            }
        }

        /// <summary>
        /// 2) INSERT DUE COLLECTION
        /// POST /api/ItemMaster/insertduecollection
        /// </summary>
        public async Task<PharmacyDueCollectionActionResponse> InsertDueCollectionAsync(PharmacyDueCollectionInsertRequest request)
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
        /// 3) UPDATE DUE COLLECTION
        /// POST /api/ItemMaster/updateduecollection
        /// </summary>
        public async Task<PharmacyDueCollectionActionResponse> UpdateDueCollectionAsync(PharmacyDueCollectionInsertRequest request)
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
        /// 4) DELETE DUE COLLECTION (soft delete, restores bill balance)
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
        /// 5) GET ALL DUE COLLECTIONS
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
        /// 6) GET DUE COLLECTION BY CODE
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
