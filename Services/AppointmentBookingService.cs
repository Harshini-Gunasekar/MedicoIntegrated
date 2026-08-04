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
                var slots = response ?? new List<AvailableSlotModel>();
                foreach (var s in slots)
                {
                    s.slot_start_time = Booking.Helpers.DateTimeExtensions.ToIndianTime(s.slot_start_time);
                    s.slot_end_time = Booking.Helpers.DateTimeExtensions.ToIndianTime(s.slot_end_time);
                }
                return slots;
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
                // Convert slot times to UTC for backend API requirement
                var apiPayload = new AppointmentBookingModel
                {
                    booking_id = booking.booking_id,
                    booking_no = booking.booking_no,
                    custid = booking.custid,
                    dcode = booking.dcode,
                    slot_detail_id = booking.slot_detail_id,
                    slot_master_id = booking.slot_master_id,
                    appointment_date = booking.appointment_date,
                    slot_start_time = Booking.Helpers.DateTimeExtensions.ToUtcFromIndianTime(booking.slot_start_time),
                    slot_end_time = Booking.Helpers.DateTimeExtensions.ToUtcFromIndianTime(booking.slot_end_time),
                    token_no = booking.token_no,
                    booking_type = booking.booking_type,
                    booking_status = booking.booking_status,
                    rescheduled_from = booking.rescheduled_from,
                    reschedule_reason = booking.reschedule_reason,
                    cancel_reason = booking.cancel_reason,
                    cancelled_at = booking.cancelled_at,
                    notes = booking.notes,
                    tenant_code = booking.tenant_code,
                    isdeleted = booking.isdeleted,
                    created_at = booking.created_at,
                    updated_at = booking.updated_at,
                    patient_name = booking.patient_name,
                    mobile = booking.mobile,
                    isvip = booking.isvip,
                    is_vip = booking.is_vip,
                    viprole = booking.viprole
                };

                var jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(apiPayload, Newtonsoft.Json.Formatting.Indented);
                Console.WriteLine("=================== POST api/AppointmentBooking/book PAYLOAD (UTC) ===================");
                Console.WriteLine(jsonPayload);
                Console.WriteLine("======================================================================================");

                var response = await _http.PostAsJsonAsync("api/AppointmentBooking/book", apiPayload);
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
                var bookings = response ?? new List<AppointmentBookingModel>();
                foreach (var b in bookings)
                {
                    b.slot_start_time = Booking.Helpers.DateTimeExtensions.ToIndianTime(b.slot_start_time);
                    b.slot_end_time = Booking.Helpers.DateTimeExtensions.ToIndianTime(b.slot_end_time);
                }
                return bookings;
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
                var bookings = response ?? new List<AppointmentBookingModel>();
                foreach (var b in bookings)
                {
                    b.slot_start_time = Booking.Helpers.DateTimeExtensions.ToIndianTime(b.slot_start_time);
                    b.slot_end_time = Booking.Helpers.DateTimeExtensions.ToIndianTime(b.slot_end_time);
                }
                return bookings;
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
                if (request?.new_booking != null)
                {
                    request.new_booking.slot_start_time = Booking.Helpers.DateTimeExtensions.ToUtcFromIndianTime(request.new_booking.slot_start_time);
                    request.new_booking.slot_end_time = Booking.Helpers.DateTimeExtensions.ToUtcFromIndianTime(request.new_booking.slot_end_time);
                }
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
                if (request?.new_booking != null)
                {
                    request.new_booking.slot_start_time = Booking.Helpers.DateTimeExtensions.ToUtcFromIndianTime(request.new_booking.slot_start_time);
                    request.new_booking.slot_end_time = Booking.Helpers.DateTimeExtensions.ToUtcFromIndianTime(request.new_booking.slot_end_time);
                }
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
                var jsonOptions = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                var jsonPayload = System.Text.Json.JsonSerializer.Serialize(request, jsonOptions);
                Console.WriteLine("--- CREATE OP PAYLOAD ---");
                Console.WriteLine(jsonPayload);
                Console.WriteLine("-------------------------");

                var response = await _http.PostAsJsonAsync("api/OpRegistration/create", request);
                var rawResponse = await response.Content.ReadAsStringAsync();

                Console.WriteLine("--- CREATE OP RESPONSE ---");
                Console.WriteLine(rawResponse);
                Console.WriteLine("--------------------------");

                return rawResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error registering OP: {ex.Message}");
                return $"Error|{ex.Message}";
            }
        }

        public async Task<string> DirectWalkinAsync(OPRegistrationModel.DirectWalkinRequest request)
        {
            try
            {
                var jsonOptions = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                var jsonPayload = System.Text.Json.JsonSerializer.Serialize(request, jsonOptions);
                Console.WriteLine("--- DIRECT WALKIN PAYLOAD ---");
                Console.WriteLine(jsonPayload);
                Console.WriteLine("-----------------------------");

                var response = await _http.PostAsJsonAsync("api/OpRegistration/direct-walkin", request);
                var rawResponse = await response.Content.ReadAsStringAsync();

                Console.WriteLine("--- DIRECT WALKIN RESPONSE ---");
                Console.WriteLine(rawResponse);
                Console.WriteLine("------------------------------");

                return rawResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error doing direct walkin: {ex.Message}");
                return $"Error|{ex.Message}";
            }
        }

        public async Task<string> DressingRegistrationAsync(OPRegistrationModel.DressingRegistrationRequest request)
        {
            try
            {
                var jsonOptions = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                var jsonPayload = System.Text.Json.JsonSerializer.Serialize(request, jsonOptions);
                Console.WriteLine("--- DRESSING REGISTRATION PAYLOAD ---");
                Console.WriteLine(jsonPayload);
                Console.WriteLine("-----------------------------");

                var response = await _http.PostAsJsonAsync("api/OpRegistration/dressing", request);
                var rawResponse = await response.Content.ReadAsStringAsync();

                Console.WriteLine("--- DRESSING REGISTRATION RESPONSE ---");
                Console.WriteLine(rawResponse);
                Console.WriteLine("------------------------------");

                return rawResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error doing dressing registration: {ex.Message}");
                return $"Error|{ex.Message}";
            }
        }

        public async Task<bool> SaveOpVitalsAsync(OPRegistrationModel.PatientVitalsModel request)
        {
            try
            {
                var jsonOptions = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                var jsonPayload = System.Text.Json.JsonSerializer.Serialize(request, jsonOptions);
                Console.WriteLine("--- SAVE VITALS PAYLOAD ---");
                Console.WriteLine(jsonPayload);
                Console.WriteLine("---------------------------");

                var response = await _http.PostAsJsonAsync("api/OpRegistration/save-vitals", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving OP vitals: {ex.Message}");
                return false;
            }
        }

        public async Task<List<OPRegistrationModel.PatientVitalsModel>> GetOpVitalsAsync(Guid? opId, Guid? ipId = null)
        {
            try
            {
                var queryParams = new List<string>();
                if (opId.HasValue && opId.Value != Guid.Empty)
                {
                    queryParams.Add($"op_id={opId.Value}");
                }
                if (ipId.HasValue && ipId.Value != Guid.Empty)
                {
                    queryParams.Add($"ip_id={ipId.Value}");
                }
                string queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
                var response = await _http.GetFromJsonAsync<List<OPRegistrationModel.PatientVitalsModel>>($"api/OpRegistration/vitals/all{queryString}");
                return response ?? new List<OPRegistrationModel.PatientVitalsModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting OP/IP vitals: {ex.Message}");
                return new List<OPRegistrationModel.PatientVitalsModel>();
            }
        }

        public async Task<bool> UpdateOpVitalsAsync(OPRegistrationModel.PatientVitalsModel request)
        {
            try
            {
                var jsonOptions = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                var jsonPayload = System.Text.Json.JsonSerializer.Serialize(request, jsonOptions);
                Console.WriteLine("--- UPDATE VITALS PAYLOAD ---");
                Console.WriteLine(jsonPayload);
                Console.WriteLine("-----------------------------");

                var response = await _http.PostAsJsonAsync("api/OpRegistration/update-vitals", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating OP vitals: {ex.Message}");
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

        public async Task<List<DoctorBookingListModel>> GetDoctorBookingsAsync(int dcode, DateOnly appointmentDate)
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<DoctorBookingListModel>>($"api/OpRegistration/doctor-bookings?dcode={dcode}&appointment_date={appointmentDate:yyyy-MM-dd}");
                return response ?? new List<DoctorBookingListModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting doctor bookings: {ex.Message}");
                return new List<DoctorBookingListModel>();
            }
        }
    }
}
