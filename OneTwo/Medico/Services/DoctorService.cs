using System.Net.Http.Json;
using Booking.Models;

namespace Booking.Services
{
    public class DoctorService
    {
        private readonly HttpClient _http;

        public DoctorService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<DoctorMasterModel>> GetDoctorsAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<DoctorMasterModel>>("api/DoctorMaster/get");
                return response ?? new List<DoctorMasterModel>();
            }
            catch
            {
                return new List<DoctorMasterModel>();
            }
        }

        public async Task<DoctorMasterModel?> GetDoctorByCodeAsync(string dcode)
        {
            try
            {
                return await _http.GetFromJsonAsync<DoctorMasterModel>($"api/DoctorMaster/get-by-dcode?dcode={dcode}");
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> InsertDoctorAsync(DoctorMasterModel doctor)
        {
            var response = await _http.PostAsJsonAsync("api/DoctorMaster/insert", doctor);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateDoctorAsync(DoctorMasterModel doctor)
        {
            var response = await _http.PostAsJsonAsync($"api/DoctorMaster/update?dcode={doctor.dcode}", doctor);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteDoctorAsync(string dcode)
        {
            var response = await _http.GetAsync($"api/DoctorMaster/delete?dcode={dcode}");
            return response.IsSuccessStatusCode;
        }
    }
}
