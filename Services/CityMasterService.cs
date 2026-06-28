using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Medico_Backend.Model;

namespace Booking.Services
{
    public class CityMasterService
    {
        private readonly HttpClient _http;

        public CityMasterService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<CityMasterModel>> GetCitiesAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<CityMasterModel>>("api/CityMaster/get");
                return response ?? new List<CityMasterModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching cities: {ex.Message}");
                return new List<CityMasterModel>();
            }
        }

        public async Task<CityMasterModel?> GetCityByCodeAsync(int citycode)
        {
            try
            {
                return await _http.GetFromJsonAsync<CityMasterModel>($"api/CityMaster/get-by-citycode?citycode={citycode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching city by code: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> InsertCityAsync(CityMasterModel city)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/CityMaster/insert", city);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting city: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateCityAsync(CityMasterModel city)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/CityMaster/update", city);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating city: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteCityAsync(int citycode)
        {
            try
            {
                // Using GetAsync since the backend's delete endpoints use GET requests
                var response = await _http.GetAsync($"api/CityMaster/delete?citycode={citycode}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting city: {ex.Message}");
                return false;
            }
        }
    }
}
