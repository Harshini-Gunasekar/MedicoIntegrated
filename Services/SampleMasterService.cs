using System.Net.Http.Json;
using Booking.Models;

namespace Booking.Services
{
    public class SampleMasterService
    {
        private readonly HttpClient _http;

        public SampleMasterService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<SampleMasterModel>> GetSampleMastersAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<SampleMasterModel>>("api/SampleMaster/get");
                return response ?? new List<SampleMasterModel>();
            }
            catch
            {
                return new List<SampleMasterModel>();
            }
        }

        public async Task<bool> InsertSampleMasterAsync(SampleMasterModel sample)
        {
            var response = await _http.PostAsJsonAsync("api/SampleMaster/insert", sample);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateSampleMasterAsync(SampleMasterModel sample)
        {
            var response = await _http.PostAsJsonAsync("api/SampleMaster/update", sample);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteSampleMasterAsync(decimal scode)
        {
            var response = await _http.GetAsync($"api/SampleMaster/delete?scode={scode}");
            return response.IsSuccessStatusCode;
        }
    }
}
