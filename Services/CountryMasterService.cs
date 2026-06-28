using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Medico_Backend.Model;

namespace Booking.Services
{
    public class CountryMasterService
    {
        private readonly HttpClient _http;

        public CountryMasterService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<CountryMasterModel>> GetCountriesAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<CountryMasterModel>>("api/CountryMaster/get");
                return response ?? new List<CountryMasterModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching countries: {ex.Message}");
                return new List<CountryMasterModel>();
            }
        }

        public async Task<CountryMasterModel?> GetCountryByCodeAsync(int countrycode)
        {
            try
            {
                return await _http.GetFromJsonAsync<CountryMasterModel>($"api/CountryMaster/get-by-countrycode?countrycode={countrycode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching country by code: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> InsertCountryAsync(CountryMasterModel country)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/CountryMaster/insert", country);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting country: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateCountryAsync(CountryMasterModel country)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/CountryMaster/update", country);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating country: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteCountryAsync(int countrycode)
        {
            try
            {
                // Using GetAsync since the backend's delete endpoints use GET requests
                var response = await _http.GetAsync($"api/CountryMaster/delete?countrycode={countrycode}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting country: {ex.Message}");
                return false;
            }
        }
    }
}
