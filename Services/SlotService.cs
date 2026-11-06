using System.Net.Http.Json;
using Booking.Models;

namespace Booking.Services
{
    public class SlotService
    {
        private readonly HttpClient _http;

        public SlotService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<DoctorAppointmentSlotMasterModel>> GetSlotsAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<DoctorAppointmentSlotMasterModel>>("api/DoctorAppointmentSlot/master/get");
                return response ?? new List<DoctorAppointmentSlotMasterModel>();
            }
            catch
            {
                return new List<DoctorAppointmentSlotMasterModel>();
            }
        }

        public async Task<List<DoctorAppointmentSlotMasterModel>> GetSlotsByDoctorAsync(int dcode)
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<DoctorAppointmentSlotMasterModel>>($"api/DoctorAppointmentSlot/master/get-by-doctor?dcode={dcode}");
                return response ?? new List<DoctorAppointmentSlotMasterModel>();
            }
            catch
            {
                return new List<DoctorAppointmentSlotMasterModel>();
            }
        }

        public async Task<SlotInsertResponse> InsertSlotAsync(object slots)
        {
            var response = await _http.PostAsJsonAsync("api/DoctorAppointmentSlot/master/insert", slots);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<SlotInsertResponse>() ?? new SlotInsertResponse();
            }
            return new SlotInsertResponse { failed = new List<object> { "API Error" } };
        }

        public async Task<bool> UpdateSlotAsync(object slots)
        {
            var response = await _http.PostAsJsonAsync("api/DoctorAppointmentSlot/master/update", slots);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteSlotAsync(Guid slot_master_id)
        {
            var response = await _http.GetAsync($"api/DoctorAppointmentSlot/master/delete?slot_master_id={slot_master_id}");
            return response.IsSuccessStatusCode;
        }
    }
}
