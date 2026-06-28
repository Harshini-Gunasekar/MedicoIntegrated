using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Medico_Backend.Model;

namespace Booking.Services
{
    public class StateMasterService
    {
        private readonly HttpClient _http;

        public StateMasterService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<StateMasterModel>> GetStatesAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<StateMasterModel>>("api/StateMaster/get");
                return response ?? new List<StateMasterModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching states: {ex.Message}");
                return new List<StateMasterModel>();
            }
        }

        public async Task<StateMasterModel?> GetStateByCodeAsync(int statecode)
        {
            try
            {
                return await _http.GetFromJsonAsync<StateMasterModel>($"api/StateMaster/get-by-statecode?statecode={statecode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching state by code: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> InsertStateAsync(StateMasterModel state)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/StateMaster/insert", state);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting state: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateStateAsync(StateMasterModel state)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/StateMaster/update", state);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating state: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteStateAsync(int statecode)
        {
            try
            {
                // Using GetAsync since the backend's delete endpoints use GET requests
                var response = await _http.GetAsync($"api/StateMaster/delete?statecode={statecode}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting state: {ex.Message}");
                return false;
            }
        }
    }
}
