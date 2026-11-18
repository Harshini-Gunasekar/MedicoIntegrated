using System.Net.Http.Json;
using Booking.Models;

namespace Booking.Services
{
    public class DoctorAppointmentSlotTypeService
    {
        private readonly HttpClient _http;

        public DoctorAppointmentSlotTypeService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<DoctorAppointmentSlotTypeModel>> GetSlotTypesAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<DoctorAppointmentSlotTypeModel>>("api/DoctorAppointmentSlotType/get");
                return response ?? new List<DoctorAppointmentSlotTypeModel>();
            }
            catch
            {
                return new List<DoctorAppointmentSlotTypeModel>();
            }
        }

        public async Task<bool> InsertSlotTypeAsync(DoctorAppointmentSlotTypeModel slotType)
        {
            var response = await _http.PostAsJsonAsync("api/DoctorAppointmentSlotType/insert", slotType);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateSlotTypeAsync(DoctorAppointmentSlotTypeModel slotType)
        {
            var response = await _http.PostAsJsonAsync("api/DoctorAppointmentSlotType/update", slotType);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteSlotTypeAsync(long slotTypeId)
        {
            var response = await _http.GetAsync($"api/DoctorAppointmentSlotType/delete?slot_type_id={slotTypeId}");
            return response.IsSuccessStatusCode;
        }
    }
}
