using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Medico_Backend.Model;

namespace Booking.Services
{
    public class AreaMasterService
    {
        private readonly HttpClient _http;

        public AreaMasterService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<AreaMasterModel>> GetAreasAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<AreaMasterModel>>("api/AreaMaster/get");
                return response ?? new List<AreaMasterModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching areas: {ex.Message}");
                return new List<AreaMasterModel>();
            }
        }

        public async Task<bool> InsertAreaAsync(AreaMasterModel area)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/AreaMaster/insert", area);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting area: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateAreaAsync(AreaMasterModel area)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/AreaMaster/update", area);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating area: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteAreaAsync(int areacode)
        {
            try
            {
                var response = await _http.GetAsync($"api/AreaMaster/delete?areacode={areacode}");
                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error response deleting area: Status={response.StatusCode}, Body={content}");
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting area: {ex.Message}");
                return false;
            }
        }
    }
}
