using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Booking.Models;

namespace Booking.Services
{
    public class TestMasterService
    {
        private readonly HttpClient _http;

        public TestMasterService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<TestMasterModel>> GetTestMastersAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<TestMasterModel>>("api/Test/get");
                return response ?? new List<TestMasterModel>();
            }
            catch
            {
                return new List<TestMasterModel>();
            }
        }

        public async Task<bool> InsertTestMasterAsync(TestMasterModel test)
        {
            var response = await _http.PostAsJsonAsync("api/Test/insert", test);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateTestMasterAsync(TestMasterModel test)
        {
            var response = await _http.PostAsJsonAsync("api/Test/update", test);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteTestMasterAsync(decimal tcode)
        {
            var response = await _http.GetAsync($"api/Test/softdelete?tcode={tcode}");
            return response.IsSuccessStatusCode;
        }
    }
}
