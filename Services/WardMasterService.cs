using System.Net.Http.Json;
using Medico_Backend.Model;

namespace Booking.Services
{
    public class WardMasterService
    {
        private readonly HttpClient _http;

        public WardMasterService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<WardMasterModel>> GetWardMastersAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<WardMasterModel>>("api/WardMaster/get");
                return response ?? new List<WardMasterModel>();
            }
            catch
            {
                return new List<WardMasterModel>();
            }
        }

        public async Task<bool> InsertWardMasterAsync(WardMasterModel ward)
        {
            var response = await _http.PostAsJsonAsync("api/WardMaster/insert", ward);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateWardMasterAsync(WardMasterModel ward)
        {
            var response = await _http.PostAsJsonAsync("api/WardMaster/update", ward);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteWardMasterAsync(int wrdcode)
        {
            var response = await _http.GetAsync($"api/WardMaster/delete?wrdcode={wrdcode}");
            return response.IsSuccessStatusCode;
        }
    }
}
