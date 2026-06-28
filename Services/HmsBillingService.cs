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
                var response = await _http.PostAsJsonAsync("api/HmsBilling/counter/open-shift", request);
                var rawResponse = await response.Content.ReadAsStringAsync();
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
    }
}
