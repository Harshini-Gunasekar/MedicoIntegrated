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
                var response = await _http.PostAsJsonAsync("api/HmsBilling/save-bill", bill);
                var rawResponse = await response.Content.ReadAsStringAsync();
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
    }
}
