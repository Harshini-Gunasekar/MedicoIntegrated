using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Medico_Backend.Model;

namespace Booking.Services
{
    public class FeetypeService
    {
        private readonly HttpClient _http;

        public FeetypeService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<FeeTypeMasterModel>> GetFeeTypesAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<FeeTypeMasterModel>>("api/FeeTypeMaster/get");
                return response ?? new List<FeeTypeMasterModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching fee types: {ex.Message}");
                return new List<FeeTypeMasterModel>();
            }
        }

        public async Task<bool> InsertFeeTypeAsync(FeeTypeMasterModel feeType)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/FeeTypeMaster/insert", feeType);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting fee type: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateFeeTypeAsync(FeeTypeMasterModel feeType)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/FeeTypeMaster/update", feeType);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating fee type: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteFeeTypeAsync(int ftcode)
        {
            try
            {
                var response = await _http.GetAsync($"api/FeeTypeMaster/delete?ftcode={ftcode}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting fee type: {ex.Message}");
                return false;
            }
        }
    }
}
