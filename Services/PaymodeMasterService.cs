using System.Net.Http.Json;
using Booking.Models;

namespace Booking.Services
{
    public class PaymodeMasterService
    {
        private readonly HttpClient _http;

        public PaymodeMasterService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<PaymodeMasterModel>> GetPaymodeMastersAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<PaymodeMasterModel>>("api/PaymodeMaster/get");
                return response ?? new List<PaymodeMasterModel>();
            }
            catch
            {
                return new List<PaymodeMasterModel>();
            }
        }

        public async Task<bool> InsertPaymodeMasterAsync(PaymodeMasterModel paymode)
        {
            var response = await _http.PostAsJsonAsync("api/PaymodeMaster/insert", paymode);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdatePaymodeMasterAsync(PaymodeMasterModel paymode)
        {
            var response = await _http.PostAsJsonAsync("api/PaymodeMaster/update", paymode);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeletePaymodeMasterAsync(decimal pmcode)
        {
            var response = await _http.GetAsync($"api/PaymodeMaster/delete?pmcode={pmcode}");
            return response.IsSuccessStatusCode;
        }
    }
}
