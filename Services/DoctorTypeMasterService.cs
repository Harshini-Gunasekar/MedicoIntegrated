using System.Net.Http.Json;
using Booking.Models;

namespace Booking.Services
{
    public class DoctorTypeMasterService
    {
        private readonly HttpClient _http;

        public DoctorTypeMasterService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<DoctorTypeMasterModel>> GetDoctorTypesAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<DoctorTypeMasterModel>>("api/DoctorTypeMaster/get");
                return response ?? new List<DoctorTypeMasterModel>();
            }
            catch
            {
                return new List<DoctorTypeMasterModel>();
            }
        }

        public async Task<bool> InsertDoctorTypeAsync(DoctorTypeMasterModel doctorType)
        {
            var response = await _http.PostAsJsonAsync("api/DoctorTypeMaster/insert", doctorType);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateDoctorTypeAsync(DoctorTypeMasterModel doctorType)
        {
            var response = await _http.PostAsJsonAsync("api/DoctorTypeMaster/update", doctorType);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteDoctorTypeAsync(int tcode)
        {
            var response = await _http.GetAsync($"api/DoctorTypeMaster/delete?tcode={tcode}");
            return response.IsSuccessStatusCode;
        }
    }
}
