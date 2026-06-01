using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Booking.Models;

namespace Booking.Services
{
    public class AppointmentBookingService
    {
        private readonly HttpClient _http;

        public AppointmentBookingService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<AvailableSlotModel>> GetAvailableSlotsAsync(int dcode, DateOnly appointmentDate)
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<AvailableSlotModel>>($"api/AppointmentBooking/get-available-slots?dcode={dcode}&appointment_date={appointmentDate:yyyy-MM-dd}");
                return response ?? new List<AvailableSlotModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting available slots: {ex.Message}");
                return new List<AvailableSlotModel>();
            }
        }

        public async Task<string> BookAppointmentAsync(AppointmentBookingModel booking)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/AppointmentBooking/book", booking);
                var result = await response.Content.ReadAsStringAsync();
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error booking appointment: {ex.Message}");
                return $"Error|{ex.Message}";
            }
        }

        public async Task<List<AppointmentBookingModel>> GetAllBookingsAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<AppointmentBookingModel>>("api/AppointmentBooking/get-all");
                return response ?? new List<AppointmentBookingModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting all bookings: {ex.Message}");
                return new List<AppointmentBookingModel>();
            }
        }

        public async Task<List<AppointmentBookingModel>> GetBookingsByDateAsync(DateOnly appointmentDate)
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<AppointmentBookingModel>>($"api/AppointmentBooking/get-by-date?appointment_date={appointmentDate:yyyy-MM-dd}");
                return response ?? new List<AppointmentBookingModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting bookings by date: {ex.Message}");
                return new List<AppointmentBookingModel>();
            }
        }

        public async Task<bool> CancelAppointmentAsync(CancelAppointmentRequest request)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/AppointmentBooking/cancel", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cancelling appointment: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateStatusAsync(Guid bookingId, string status)
        {
            try
            {
                var response = await _http.PostAsync($"api/AppointmentBooking/update-status?booking_id={bookingId}&booking_status={status}", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating status: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RescheduleAppointmentAsync(RescheduleAppointmentRequest request)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/AppointmentBooking/reschedule", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error rescheduling appointment: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RescheduleWholeSlotAsync(RescheduleWholeSlotRequest request)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/AppointmentBooking/reschedule-whole-slot", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error rescheduling whole slot: {ex.Message}");
                return false;
            }
        }

        public async Task<List<AppointmentBookingModel>> GetBookingsByCustomerAsync(decimal custid)
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<AppointmentBookingModel>>($"api/AppointmentBooking/by-customer?custid={custid}");
                return response ?? new List<AppointmentBookingModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting bookings by customer: {ex.Message}");
                return new List<AppointmentBookingModel>();
            }
        }

        public async Task<bool> PatientRescheduleAppointmentAsync(RescheduleAppointmentRequest request)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/AppointmentBooking/patient-reschedule", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error patient rescheduling appointment: {ex.Message}");
                return false;
            }
        }

        public async Task<string> RegisterOpAsync(OPRegistrationModel.OpRegistrationModel request)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/OpRegistration/create", request);
                var rawResponse = await response.Content.ReadAsStringAsync();
                return rawResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error registering OP: {ex.Message}");
                return $"Error|{ex.Message}";
            }
        }

        public async Task<bool> SaveOpVitalsAsync(OPRegistrationModel.PatientVitalsModel request)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/OpRegistration/save-vitals", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving OP vitals: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateOpStatusAsync(Guid opId, string visitStatus)
        {
            try
            {
                var response = await _http.PostAsync($"api/OpRegistration/update-visit-status?op_id={opId}&visit_status={visitStatus}", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating OP status: {ex.Message}");
                return false;
            }
        }

        public async Task<List<OPRegistrationModel.OpRegistrationModel>> GetAllOpRegistrationsAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<OPRegistrationModel.OpRegistrationModel>>("api/OpRegistration/all");
                return response ?? new List<OPRegistrationModel.OpRegistrationModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting all OP registrations: {ex.Message}");
                return new List<OPRegistrationModel.OpRegistrationModel>();
            }
        }
    }
}
