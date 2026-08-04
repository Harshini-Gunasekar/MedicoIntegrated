using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Booking.Models;

namespace Booking.Services
{
    public class OpChargeSlabService
    {
        private readonly HttpClient _http;

        public OpChargeSlabService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<OpChargeSlabModel>> GetSlabsByDoctorAsync(int dcode)
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<OpChargeSlabModel>>($"api/OpChargeSlab/get?dcode={dcode}");
                return response ?? new List<OpChargeSlabModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting doctor OP charge slabs: {ex.Message}");
                return new List<OpChargeSlabModel>();
            }
        }

        public async Task<bool> AddSlabsAsync(List<OpChargeSlabModel> slabs)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/OpChargeSlab/add-list", slabs);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding doctor OP charge slabs: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateSlabsAsync(List<OpChargeSlabModel> slabs)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/OpChargeSlab/update-list", slabs);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating doctor OP charge slabs: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteSlabAsync(int slabid)
        {
            try
            {
                var response = await _http.GetAsync($"api/OpChargeSlab/delete?slabid={slabid}");
                if (!response.IsSuccessStatusCode)
                {
                    // Fallback to POST with null content if GET returns an error code
                    var postResponse = await _http.PostAsync($"api/OpChargeSlab/delete?slabid={slabid}", null);
                    return postResponse.IsSuccessStatusCode;
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting doctor OP charge slab: {ex.Message}");
                return false;
            }
        }
    }
}
