using System.Net.Http.Json;
using medico_backend.Model;

namespace Booking.Services
{
    public class BedStatusService
    {
        private readonly HttpClient _http;

        public BedStatusService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<BedStatusModel>> GetBedStatusesAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<BedStatusModel>>("api/BedStatus/get");
                return response ?? new List<BedStatusModel>();
            }
            catch
            {
                return new List<BedStatusModel>();
            }
        }

        public async Task<BedStatusModel?> GetBedStatusByBedAsync(int bedcode)
        {
            try
            {
                var response = await _http.GetFromJsonAsync<BedStatusModel>($"api/BedStatus/get-by-bed?bedcode={bedcode}");
                return response;
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<BedStatusModel>> GetPendingCleaningBedsAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<BedStatusModel>>("api/BedStatus/pending-cleaning");
                return response ?? new List<BedStatusModel>();
            }
            catch
            {
                return new List<BedStatusModel>();
            }
        }

        public async Task<bool> MarkBedCleanedAsync(MarkBedCleanedRequest request)
        {
            var response = await _http.PostAsJsonAsync("api/BedStatus/mark-cleaned", request);
            return response.IsSuccessStatusCode;
        }
    }
}
