using System.Net.Http.Json;
using Medico_Backend.Model;

namespace Booking.Services
{
    public class AppointmentPreBookingService
    {
        private readonly HttpClient _http;

        public AppointmentPreBookingService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<AppointmentPreBookingModel>> GetPreBookingsAsync(string? date = null)
        {
            try
            {
                string url = "api/AppointmentPreBooking/get";
                if (!string.IsNullOrEmpty(date))
                {
                    url += $"?date={date}";
                }

                var response = await _http.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<List<AppointmentPreBookingModel>>(json) ?? new List<AppointmentPreBookingModel>();
                }
                return new List<AppointmentPreBookingModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AppointmentPreBookingService] Error loading pre-bookings: {ex.Message}");
                return new List<AppointmentPreBookingModel>();
            }
        }

        public async Task<bool> AddPreBookingAsync(AddAppointmentPreBookingRequest request)
        {
            try
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(request);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await _http.PostAsync("api/AppointmentPreBooking/add", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AppointmentPreBookingService] Error adding pre-booking: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdatePreBookingAsync(UpdateAppointmentPreBookingRequest request)
        {
            try
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(request);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await _http.PostAsync("api/AppointmentPreBooking/update", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AppointmentPreBookingService] Error updating pre-booking: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeletePreBookingAsync(long preferenceid)
        {
            try
            {
                var response = await _http.GetAsync($"api/AppointmentPreBooking/delete?preferenceid={preferenceid}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AppointmentPreBookingService] Error deleting pre-booking: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> MarkVisitedAsync(MarkVisitedRequest request)
        {
            try
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(request);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await _http.PostAsync("api/AppointmentPreBooking/mark-visited", content);
                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[AppointmentPreBookingService] MarkVisited failed. Status: {response.StatusCode}, Body: {errorBody}");
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AppointmentPreBookingService] Error marking visited: {ex.Message}");
                return false;
            }
        }
    }
}
