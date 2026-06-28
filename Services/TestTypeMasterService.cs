using System.Net.Http.Json;
using Booking.Models;

namespace Booking.Services
{
    public class TestTypeMasterService
    {
        private readonly HttpClient _http;

        public TestTypeMasterService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<TestTypeMasterModel>> GetTestTypeMastersAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<TestTypeMasterModel>>("api/TestTypeMaster/get");
                return response ?? new List<TestTypeMasterModel>();
            }
            catch
            {
                return new List<TestTypeMasterModel>();
            }
        }

        public async Task<bool> InsertTestTypeMasterAsync(TestTypeMasterModel testType)
        {
            var response = await _http.PostAsJsonAsync("api/TestTypeMaster/insert", testType);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateTestTypeMasterAsync(TestTypeMasterModel testType)
        {
            var response = await _http.PostAsJsonAsync("api/TestTypeMaster/update", testType);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteTestTypeMasterAsync(decimal ttid)
        {
            var response = await _http.GetAsync($"api/TestTypeMaster/delete?ttid={ttid}");
            return response.IsSuccessStatusCode;
        }
    }
}
