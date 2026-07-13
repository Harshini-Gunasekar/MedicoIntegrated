using System.Net.Http.Json;
using Medico_Backend.Model;

namespace Booking.Services
{
    public class BedTransferService
    {
        private readonly HttpClient _http;

        public BedTransferService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<BedTransferModel>> GetBedTransfersAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<BedTransferModel>>("api/BedTransfer/get");
                return response ?? new List<BedTransferModel>();
            }
            catch
            {
                return new List<BedTransferModel>();
            }
        }

        public async Task<List<BedTransferModel>> GetActiveAdmissionsAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<BedTransferModel>>("api/BedTransfer/get-active-admissions");
                return response ?? new List<BedTransferModel>();
            }
            catch
            {
                return new List<BedTransferModel>();
            }
        }

        public async Task<List<BedTransferModel>> GetBedTransfersByCustIdAsync(int custid)
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<BedTransferModel>>($"api/BedTransfer/get-by-custid?custid={custid}");
                return response ?? new List<BedTransferModel>();
            }
            catch
            {
                return new List<BedTransferModel>();
            }
        }

        public async Task<bool> InsertBedTransferAsync(BedTransferModel transfer)
        {
            var response = await _http.PostAsJsonAsync("api/BedTransfer/insert", transfer);
            return response.IsSuccessStatusCode;
        }
    }
}
