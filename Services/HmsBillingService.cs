using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Booking.Models;

namespace Booking.Services
{
    public class HmsBillingService
    {
        private readonly HttpClient _http;

        public HmsBillingService(HttpClient http)
        {
            _http = http;
        }

        public async Task<string> SaveBillAsync(HmsBillModel bill)
        {
            try
            {
                var jsonPayload = System.Text.Json.JsonSerializer.Serialize(bill, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine("--- BILL JSON PAYLOAD ---");
                Console.WriteLine(jsonPayload);
                Console.WriteLine("-------------------------");

                var response = await _http.PostAsJsonAsync("api/HmsBilling/save-bill", bill);
                var rawResponse = await response.Content.ReadAsStringAsync();
                
                Console.WriteLine($"--- API RESPONSE: {rawResponse} ---");
                
                return rawResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving bill: {ex.Message}");
                return $"Error|{ex.Message}";
            }
        }

        public async Task<HmsBillModel?> GetBillAsync(Guid opId)
        {
            try
            {
                // First try standard get-bill endpoint
                var response = await _http.GetAsync($"api/HmsBilling/get-bill?op_id={opId}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<HmsBillModel>();
                }
                
                // Fallback to searching by op_id under a list endpoint if any, or return null
                var fallbackResponse = await _http.GetAsync($"api/HmsBilling/by-op?op_id={opId}");
                if (fallbackResponse.IsSuccessStatusCode)
                {
                    return await fallbackResponse.Content.ReadFromJsonAsync<HmsBillModel>();
                }
                
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching bill: {ex.Message}");
                return null;
            }
        }

        public async Task<HmsBillListResponse?> ListBillsAsync(HmsBillFilterRequest filter)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/HmsBilling/list-bills", filter);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<HmsBillListResponse>();
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching bills list: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> AddPaymentAsync(HmsPaymentRequest request)
        {
            try
            {
                // Sanitize paymode codes: if 0, convert to null
                if (request.pmc1.HasValue && request.pmc1.Value == 0) request.pmc1 = null;
                if (request.pmc2.HasValue && request.pmc2.Value == 0) request.pmc2 = null;
                if (request.pmc3.HasValue && request.pmc3.Value == 0) request.pmc3 = null;

                // Zero out amount if corresponding pmc is null
                if (!request.pmc1.HasValue) request.pmc1_amount = null;
                if (!request.pmc2.HasValue) request.pmc2_amount = null;
                if (!request.pmc3.HasValue) request.pmc3_amount = null;

                // Ensure reference_no and bank_name are non-null strings
                request.reference_no ??= "";
                request.bank_name ??= "";

                var response = await _http.PostAsJsonAsync("api/HmsBilling/add-payment", request);
                if (!response.IsSuccessStatusCode)
                {
                    var errText = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[AddPaymentAsync] Failed ({response.StatusCode}): {errText}");
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding payment: {ex.Message}");
                return false;
            }
        }

        public async Task<HmsBillResponse?> GetBillByGuidAsync(string guid)
        {
            try
            {
                var response = await _http.GetAsync($"api/HmsBilling/get-bill/{guid}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<HmsBillResponse>();
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching bill by guid: {ex.Message}");
                return null;
            }
        }
        public async Task<string> UpdateBillAsync(HmsBillModel model)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/HmsBilling/update-bill", model);
                var rawResponse = await response.Content.ReadAsStringAsync();
                
                Console.WriteLine($"--- API RESPONSE (UPDATE): {rawResponse} ---");
                
                return rawResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating bill: {ex.Message}");
                return $"Error|{ex.Message}";
            }
        }

        public async Task<string> CancelBillAsync(string requestguid, int usercode, string reason)
        {
            try
            {
                var payload = new { requestguid, usercode, reason };
                var response = await _http.PostAsJsonAsync("api/HmsBilling/cancel-bill", payload);
                var rawResponse = await response.Content.ReadAsStringAsync();
                
                Console.WriteLine($"--- API RESPONSE (CANCEL): {rawResponse} ---");
                
                return rawResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cancelling bill: {ex.Message}");
                return $"Error|{ex.Message}";
            }
        }

        public async Task<string> OpenShiftAsync(OpenShiftRequest request)
        {
            try
            {
                var jsonPayload = System.Text.Json.JsonSerializer.Serialize(request, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine("--- OPEN SHIFT JSON PAYLOAD ---");
                Console.WriteLine(jsonPayload);

                var response = await _http.PostAsJsonAsync("api/HmsBilling/counter/open-shift", request);
                var rawResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine("--- OPEN SHIFT API RESPONSE ---");
                Console.WriteLine(rawResponse);
                return rawResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening shift: {ex.Message}");
                return $"Error|{ex.Message}";
            }
        }

        public async Task<string> CloseShiftAsync(CloseShiftRequest request)
        {
            try
            {
                var jsonPayload = System.Text.Json.JsonSerializer.Serialize(request, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine("--- CLOSE SHIFT JSON PAYLOAD ---");
                Console.WriteLine(jsonPayload);

                var response = await _http.PostAsJsonAsync("api/HmsBilling/counter/close-shift", request);
                var rawResponse = await response.Content.ReadAsStringAsync();
                return rawResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error closing shift: {ex.Message}");
                return $"Error|{ex.Message}";
            }
        }

        public async Task<BillNoListResponse?> ListBillNoConfigsAsync(BillNoListRequest request)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/HmsBilling/billno/list", request);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<BillNoListResponse>();
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching bill no configs list: {ex.Message}");
                return null;
            }
        }

        public async Task<BillNoConfig?> GetBillNoConfigAsync(int bncode)
        {
            try
            {
                var response = await _http.GetAsync($"api/HmsBilling/billno/{bncode}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<BillNoConfig>();
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching bill no config: {ex.Message}");
                return null;
            }
        }

        public async Task<string> CreateBillNoConfigAsync(BillNoConfig model)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/HmsBilling/billno/create", model);
                var rawResponse = await response.Content.ReadAsStringAsync();
                return rawResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating bill no config: {ex.Message}");
                return $"Error|{ex.Message}";
            }
        }

        public async Task<string> UpdateBillNoConfigAsync(BillNoConfig model)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/HmsBilling/billno/update", model);
                var rawResponse = await response.Content.ReadAsStringAsync();
                return rawResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating bill no config: {ex.Message}");
                return $"Error|{ex.Message}";
            }
        }

        public async Task<string> DeleteBillNoConfigAsync(int bncode, int usercode)
        {
            try
            {
                var payload = new BillNoDeleteRequest { bncode = bncode, usercode = usercode };
                var response = await _http.PostAsJsonAsync("api/HmsBilling/billno/delete", payload);
                var rawResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"--- BillNo Delete Response (bncode={bncode}) ---");
                Console.WriteLine(rawResponse);
                Console.WriteLine("------------------------------------------------");
                return rawResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting bill no config: {ex.Message}");
                return $"Error|{ex.Message}";
            }
        }

        public async Task<List<CounterTimingDto>> GetOpenCountersAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<CounterTimingDto>>("api/CounterTiming/get");
                if (response != null)
                {
                    // Filter to only include open shifts (todate is null)
                    return response.Where(x => x.todate == null).ToList();
                }
                return new List<CounterTimingDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching open counters: {ex.Message}");
                return new List<CounterTimingDto>();
            }
        }

        public async Task<List<UnbilledChargeSummary>> GetUnbilledChargesByVisitAsync(string? opvisitid = null, Guid? ip_id = null)
        {
            try
            {
                var queryParams = new List<string>();
                if (ip_id.HasValue && ip_id.Value != Guid.Empty)
                {
                    queryParams.Add($"ip_id={ip_id.Value}");
                }
                else if (!string.IsNullOrEmpty(opvisitid))
                {
                    queryParams.Add($"opvisitid={Uri.EscapeDataString(opvisitid)}");
                }

                var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
                var rawJson = await _http.GetStringAsync($"api/UnbilledCharges/by-visit{queryString}");
                return ParseUnbilledChargeSummaryList(rawJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching unbilled charges: {ex.Message}");
                return new List<UnbilledChargeSummary>();
            }
        }

        public async Task<List<UnbilledChargeSummary>> GetAllUnbilledChargesAsync(string? op_id = null, string? ip_id = null)
        {
            try
            {
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(ip_id))
                {
                    queryParams.Add($"ip_id={Uri.EscapeDataString(ip_id)}");
                }
                if (!string.IsNullOrEmpty(op_id))
                {
                    queryParams.Add($"op_id={Uri.EscapeDataString(op_id)}");
                }

                var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
                var rawJson = await _http.GetStringAsync($"api/UnbilledCharges/get-all{queryString}");
                return ParseUnbilledChargeSummaryList(rawJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching all unbilled charges: {ex.Message}");
                return new List<UnbilledChargeSummary>();
            }
        }

        public async Task<string> AddUnbilledConsultationAsync(AddUnbilledConsultationRequest request)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/UnbilledCharges/add-consultation", request);
                var rawResponse = await response.Content.ReadAsStringAsync();
                return rawResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding unbilled consultation: {ex.Message}");
                return $"Error|{ex.Message}";
            }
        }

        public async Task<List<UnbilledChargeSummary>> GetIpRoomRentSummaryAsync(Guid ipId)
        {
            try
            {
                var rawJson = await _http.GetStringAsync($"api/UnbilledCharges/ip-room-rent-summary?ip_id={ipId}");
                return ParseUnbilledChargeSummaryList(rawJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching IP room rent summary: {ex.Message}");
                return new List<UnbilledChargeSummary>();
            }
        }

        private List<UnbilledChargeSummary> ParseUnbilledChargeSummaryList(string? rawJson)
        {
            if (string.IsNullOrWhiteSpace(rawJson))
                return new List<UnbilledChargeSummary>();

            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(rawJson);
                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    return System.Text.Json.JsonSerializer.Deserialize<List<UnbilledChargeSummary>>(rawJson, options) ?? new();
                }
                else if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Object)
                {
                    if (doc.RootElement.TryGetProperty("value", out var valueProp) && valueProp.ValueKind == System.Text.Json.JsonValueKind.Array)
                    {
                        return System.Text.Json.JsonSerializer.Deserialize<List<UnbilledChargeSummary>>(valueProp.GetRawText(), options) ?? new();
                    }
                    if (doc.RootElement.TryGetProperty("data", out var dataProp) && dataProp.ValueKind == System.Text.Json.JsonValueKind.Array)
                    {
                        return System.Text.Json.JsonSerializer.Deserialize<List<UnbilledChargeSummary>>(dataProp.GetRawText(), options) ?? new();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing unbilled charges JSON: {ex.Message}");
            }
            return new List<UnbilledChargeSummary>();
        }
    }
}
