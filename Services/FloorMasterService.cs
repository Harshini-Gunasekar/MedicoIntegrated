using System.Net.Http.Json;
using Medico_Backend.Model;

namespace Booking.Services
{
    public class FloorMasterService
    {
        private readonly HttpClient _http;

        public FloorMasterService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<FloorMasterModel>> GetFloorMastersAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<FloorMasterModel>>("api/FloorMaster/get");
                return response ?? new List<FloorMasterModel>();
            }
            catch
            {
                return new List<FloorMasterModel>();
            }
        }

        public async Task<bool> InsertFloorMasterAsync(FloorMasterModel floor)
        {
            var response = await _http.PostAsJsonAsync("api/FloorMaster/insert", floor);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateFloorMasterAsync(FloorMasterModel floor)
        {
            var response = await _http.PostAsJsonAsync("api/FloorMaster/update", floor);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteFloorMasterAsync(int flrcode)
        {
            var response = await _http.GetAsync($"api/FloorMaster/delete?flrcode={flrcode}");
            return response.IsSuccessStatusCode;
        }
    }
}
