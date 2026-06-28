using System.Net.Http.Json;
using Booking.Models;

namespace Booking.Services
{
    public class ReimbursementCompanyMasterService
    {
        private readonly HttpClient _http;

        public ReimbursementCompanyMasterService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<ReimbursementCompanyMasterModel>> GetReimbursementCompaniesAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<ReimbursementCompanyMasterModel>>("api/ReimbursementCompanyMaster/get");
                return response ?? new List<ReimbursementCompanyMasterModel>();
            }
            catch
            {
                return new List<ReimbursementCompanyMasterModel>();
            }
        }

        public async Task<bool> InsertReimbursementCompanyAsync(ReimbursementCompanyMasterModel company)
        {
            var response = await _http.PostAsJsonAsync("api/ReimbursementCompanyMaster/insert", company);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateReimbursementCompanyAsync(ReimbursementCompanyMasterModel company)
        {
            var response = await _http.PostAsJsonAsync("api/ReimbursementCompanyMaster/update", company);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteReimbursementCompanyAsync(decimal ricode)
        {
            var response = await _http.GetAsync($"api/ReimbursementCompanyMaster/delete?ricode={ricode}");
            return response.IsSuccessStatusCode;
        }
    }
}
