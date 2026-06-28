using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Medico_Backend.Model;

namespace Booking.Services
{
    public class TestFeeService
    {
        private readonly HttpClient _http;

        public TestFeeService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<TestFeeMasterModel>> GetTestFeesAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<TestFeeMasterModel>>("api/TestFeeMaster/get");
                return response ?? new List<TestFeeMasterModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching test fees: {ex.Message}");
                return new List<TestFeeMasterModel>();
            }
        }

        public async Task<bool> InsertTestFeeAsync(TestFeeMasterModel testFee)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/TestFeeMaster/insert", testFee);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting test fee: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateTestFeeAsync(TestFeeMasterModel testFee)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/TestFeeMaster/update", testFee);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating test fee: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteTestFeeAsync(decimal tfcode)
        {
            try
            {
                var response = await _http.GetAsync($"api/TestFeeMaster/delete?tfcode={tfcode}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting test fee: {ex.Message}");
                return false;
            }
        }
    }
}
