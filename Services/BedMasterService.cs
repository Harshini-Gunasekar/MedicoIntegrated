using System.Net.Http.Json;
using Medico_Backend.Model;

namespace Booking.Services
{
    public class BedMasterService
    {
        private readonly HttpClient _http;

        public BedMasterService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<BedMasterModel>> GetBedMastersAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<BedMasterModel>>("api/BedMaster/get");
                return response ?? new List<BedMasterModel>();
            }
            catch
            {
                return new List<BedMasterModel>();
            }
        }

        public async Task<List<BedMasterModel>> GetPendingCleaningBedsAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<BedMasterModel>>("api/BedMaster/get-pending-cleaning");
                return response ?? new List<BedMasterModel>();
            }
            catch
            {
                return new List<BedMasterModel>();
            }
        }

        public async Task<List<BedMasterModel>> GetAvailableBedsAsync(int? blockcode = null, int? flrcode = null, int? wrdcode = null)
        {
            try
            {
                var queryParams = new List<string>();
                if (blockcode.HasValue) queryParams.Add($"blockcode={blockcode.Value}");
                if (flrcode.HasValue) queryParams.Add($"flrcode={flrcode.Value}");
                if (wrdcode.HasValue) queryParams.Add($"wrdcode={wrdcode.Value}");

                var url = "api/BedMaster/get-available";
                if (queryParams.Any())
                {
                    url += "?" + string.Join("&", queryParams);
                }

                var response = await _http.GetFromJsonAsync<List<BedMasterModel>>(url);
                return response ?? new List<BedMasterModel>();
            }
            catch
            {
                return new List<BedMasterModel>();
            }
        }

        public async Task<bool> InsertBedMasterAsync(BedMasterModel bed)
        {
            var response = await _http.PostAsJsonAsync("api/BedMaster/insert", bed);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateBedMasterAsync(BedMasterModel bed)
        {
            var response = await _http.PostAsJsonAsync("api/BedMaster/update", bed);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteBedMasterAsync(int bedcode)
        {
            var response = await _http.GetAsync($"api/BedMaster/delete?bedcode={bedcode}");
            return response.IsSuccessStatusCode;
        }
    }
}
