using System.Net.Http.Json;
using Medico_Backend.Model;

namespace Booking.Services
{
    public class NurseMasterService
    {
        private readonly HttpClient _http;

        public NurseMasterService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<NurseMasterModel>> GetNurseMastersAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<NurseMasterModel>>("api/NurseMaster/get");
                return response ?? new List<NurseMasterModel>();
            }
            catch
            {
                return new List<NurseMasterModel>();
            }
        }

        public async Task<bool> InsertNurseMasterAsync(NurseMasterModel nurse)
        {
            var response = await _http.PostAsJsonAsync("api/NurseMaster/insert", nurse);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateNurseMasterAsync(NurseMasterModel nurse)
        {
            var response = await _http.PostAsJsonAsync("api/NurseMaster/update", nurse);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteNurseMasterAsync(int ncode)
        {
            var response = await _http.GetAsync($"api/NurseMaster/delete?ncode={ncode}");
            return response.IsSuccessStatusCode;
        }
    }
}
