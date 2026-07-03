using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Medico_Backend.Model;

namespace Booking.Services
{
    public class CounterMasterService
    {
        private readonly HttpClient _http;

        public CounterMasterService(HttpClient http)
        {
            _http = http;
        }

        // --- Counter Master ---

        public async Task<List<CounterMasterModel>> GetCountersAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<CounterMasterModel>>("api/CounterMaster/get");
                return response ?? new List<CounterMasterModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching counter masters: {ex.Message}");
                return new List<CounterMasterModel>();
            }
        }

        public async Task<bool> InsertCounterAsync(CounterMasterModel counter)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/CounterMaster/insert", counter);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting counter: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateCounterAsync(CounterMasterModel counter)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/CounterMaster/update", counter);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating counter: {ex.Message}");
                return false;
            }
        }

        public async Task<string> DeleteCounterAsync(decimal cntcode)
        {
            try
            {
                var response = await _http.GetAsync($"api/CounterMaster/delete?cntcode={cntcode}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return content.Trim();
                }
                return "Error: Request failed";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting counter: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }


    }
}
