using System.Net.Http.Json;
using Booking.Models;

namespace Booking.Services
{
    public class DoctorSpecialtyMasterService
    {
        private readonly HttpClient _http;

        public DoctorSpecialtyMasterService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<DoctorSpecialtyMasterModel>> GetDoctorSpecialtiesAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<DoctorSpecialtyMasterModel>>("api/DoctorSpecialtyMaster/get");
                return response ?? new List<DoctorSpecialtyMasterModel>();
            }
            catch
            {
                return new List<DoctorSpecialtyMasterModel>();
            }
        }

        public async Task<bool> InsertDoctorSpecialtyAsync(DoctorSpecialtyMasterModel specialty)
        {
            var response = await _http.PostAsJsonAsync("api/DoctorSpecialtyMaster/insert", specialty);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateDoctorSpecialtyAsync(DoctorSpecialtyMasterModel specialty)
        {
            var response = await _http.PostAsJsonAsync("api/DoctorSpecialtyMaster/update", specialty);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteDoctorSpecialtyAsync(int spcode)
        {
            var response = await _http.GetAsync($"api/DoctorSpecialtyMaster/delete?spcode={spcode}");
            return response.IsSuccessStatusCode;
        }
    }
}
