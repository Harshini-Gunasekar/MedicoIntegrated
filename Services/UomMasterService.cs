using System.Net.Http.Json;
using Booking.Models;

namespace Booking.Services
{
    public class UomMasterService
    {
        private readonly HttpClient _http;

        public UomMasterService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<UomMasterModel>> GetUomMastersAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<UomMasterModel>>("api/UomMaster/get");
                return response ?? new List<UomMasterModel>();
            }
            catch
            {
                return new List<UomMasterModel>();
            }
        }

        public async Task<bool> InsertUomMasterAsync(UomMasterModel uom)
        {
            var response = await _http.PostAsJsonAsync("api/UomMaster/insert", uom);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateUomMasterAsync(UomMasterModel uom)
        {
            var response = await _http.PostAsJsonAsync("api/UomMaster/update", uom);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteUomMasterAsync(decimal ucode)
        {
            var response = await _http.GetAsync($"api/UomMaster/delete?ucode={ucode}");
            return response.IsSuccessStatusCode;
        }
    }
}
