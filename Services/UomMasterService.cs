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
            var list = new List<UomMasterModel>();
            try
            {
                var response = await _http.GetFromJsonAsync<List<UomMasterModel>>("api/UomMaster/get");
                if (response != null && response.Any()) list.AddRange(response);
            }
            catch { }

            try
            {
                var uomResp = await _http.GetFromJsonAsync<List<UomMasterModel>>("Uom/get");
                if (uomResp != null && uomResp.Any())
                {
                    foreach (var item in uomResp)
                    {
                        if (!list.Any(x => x.ucode == item.ucode || (x.name != null && item.name != null && x.name.Trim().Equals(item.name.Trim(), StringComparison.OrdinalIgnoreCase))))
                        {
                            list.Add(item);
                        }
                    }
                }
            }
            catch { }

            return list;
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