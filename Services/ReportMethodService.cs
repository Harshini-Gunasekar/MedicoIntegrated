using System.Net.Http.Json;
using Booking.Models;

namespace Booking.Services
{
    public class ReportMethodService
    {
        private readonly HttpClient _http;

        public ReportMethodService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<ReportMethodModel>> GetReportMethodsAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<ReportMethodModel>>("api/ReportMethod/get");
                return response ?? new List<ReportMethodModel>();
            }
            catch
            {
                return new List<ReportMethodModel>();
            }
        }

        public async Task<bool> InsertReportMethodAsync(ReportMethodModel method)
        {
            var response = await _http.PostAsJsonAsync("api/ReportMethod/insert", method);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateReportMethodAsync(ReportMethodModel method)
        {
            var response = await _http.PostAsJsonAsync("api/ReportMethod/update", method);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteReportMethodAsync(decimal rtmcode)
        {
            var response = await _http.GetAsync($"api/ReportMethod/delete?rmtcode={rtmcode}");
            return response.IsSuccessStatusCode;
        }
    }
}
