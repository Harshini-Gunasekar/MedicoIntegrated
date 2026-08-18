using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Booking.Helpers;
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
                var response = await _http.GetAsync($"api/AppointmentBooking/get-available-slots?dcode={dcode}&appointment_date={appointmentDate:yyyy-MM-dd}");
                if (response.IsSuccessStatusCode)
                {
                    var slots = await response.Content.ReadFromJsonAsync<List<AvailableSlotModel>>() ?? new List<AvailableSlotModel>();
                    foreach (var s in slots)
                    {
                        s.slot_start_time = Booking.Helpers.DateTimeExtensions.ToIndianTime(s.slot_start_time);
                        s.slot_end_time = Booking.Helpers.DateTimeExtensions.ToIndianTime(s.slot_end_time);
                    }
                    return slots;
                }
                return new List<AvailableSlotModel>();
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
                var normalizedBookingType = string.IsNullOrWhiteSpace(booking.booking_type) || booking.booking_type.Equals("WALKIN", StringComparison.OrdinalIgnoreCase)
                    ? "ONLINE"
                    : booking.booking_type;

                var normalizedNotes = booking.notes;
                if (!string.IsNullOrWhiteSpace(normalizedNotes) && normalizedNotes.StartsWith("[WALKIN]", StringComparison.OrdinalIgnoreCase))
                {
                    normalizedNotes = normalizedNotes.Substring("[WALKIN]".Length).Trim();
                }

                booking.booking_type = normalizedBookingType;
                booking.notes = string.IsNullOrWhiteSpace(normalizedNotes) ? null : normalizedNotes;

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
                    booking_type = normalizedBookingType,
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

        public async Task<(string BookingResult, string? OpResult, int TokenNo, Guid BookingId)> BookAppointmentWithOpRegistrationAsync(AppointmentBookingModel booking, int? departmentCode = null)
        {
            var bookingResult = await BookAppointmentAsync(booking);
            string? opResult = null;
            int tokenNo = booking.token_no;
            Guid bookingId = booking.booking_id;

            if (bookingResult.StartsWith("Success", StringComparison.OrdinalIgnoreCase))
            {
                var parts = bookingResult.Split('|');
                foreach (var part in parts)
                {
                    if (part.StartsWith("Token:", StringComparison.OrdinalIgnoreCase) && int.TryParse(part.Substring(6), out var t))
                    {
                        tokenNo = t;
                    }
                    else if (part.StartsWith("BookingId:", StringComparison.OrdinalIgnoreCase) && Guid.TryParse(part.Substring(10), out var g))
                    {
                        bookingId = g;
                    }
                }

                try
                {
                    var savedBooking = await GetBookingByIdAsync(bookingId);
                    if (savedBooking == null)
                    {
                        savedBooking = booking;
                        savedBooking.booking_id = bookingId;
                        savedBooking.booking_no ??= booking.booking_no;
                        savedBooking.booking_type = "ONLINE";
                        savedBooking.notes = string.IsNullOrWhiteSpace(booking.notes) ? null : booking.notes.Replace("[WALKIN]", string.Empty, StringComparison.OrdinalIgnoreCase).Trim();
                    }
                    else
                    {
                        savedBooking.booking_type = "ONLINE";
                        if (!string.IsNullOrWhiteSpace(savedBooking.notes))
                        {
                            savedBooking.notes = savedBooking.notes.Replace("[WALKIN]", string.Empty, StringComparison.OrdinalIgnoreCase).Trim();
                        }
                    }

                    var opRegistration = new OPRegistrationModel.OpRegistrationModel
                    {
                        op_id = Guid.NewGuid(),
                        op_no = "",
                        booking_id = savedBooking.booking_id,
                        booking_no = savedBooking.booking_no ?? booking.booking_no ?? "",
                        slot_detail_id = savedBooking.slot_detail_id,
                        custid = savedBooking.custid,
                        dcode = savedBooking.dcode,
                        department_code = departmentCode ?? 3,
                        visit_type = "NEWVISIT",
                        reg_type = "ONLINE",
                        visit_date = savedBooking.appointment_date,
                        token_no = tokenNo > 0 ? tokenNo : null,
                        queue_no = null,
                        visit_status = "WAITING",
                        notes = savedBooking.notes,
                        tenant_code = savedBooking.tenant_code ?? booking.tenant_code,
                        isdeleted = false,
                        created_at = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                        updated_at = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                        is_direct_walkin = false,
                        duty_dcode = null,
                        transferred_to_dcode = null,
                        transfer_reason = null,
                        is_dressing = false
                    };

                    opResult = await RegisterOpAsync(opRegistration);
                    Console.WriteLine($"[AppointmentBookingService] OP Registration response: {opResult}");

                    if (opResult != null && opResult.StartsWith("Success", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            var allOps = await GetAllOpRegistrationsAsync();
                            var match = allOps.FirstOrDefault(x => (savedBooking.booking_id != Guid.Empty && x.booking_id == savedBooking.booking_id) || x.op_id == opRegistration.op_id);
                            if (match != null && match.token_no.HasValue && match.token_no.Value > 0)
                            {
                                tokenNo = match.token_no.Value;
                            }
                        }
                        catch (Exception fetchEx)
                        {
                            Console.WriteLine($"[AppointmentBookingService] Error fetching updated OP token: {fetchEx.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[AppointmentBookingService] Error creating OP Registration: {ex.Message}");
                    opResult = $"Error|{ex.Message}";
                }
            }

            return (bookingResult, opResult, tokenNo, bookingId);
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

        public async Task<AppointmentBookingModel?> GetBookingByIdAsync(Guid bookingId)
        {
            try
            {
                var bookings = await GetAllBookingsAsync();
                return bookings.FirstOrDefault(b => b.booking_id == bookingId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting booking by id: {ex.Message}");
                return null;
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
                request.reg_type = "ONLINE";
                request.is_direct_walkin = false;
                if (!string.IsNullOrWhiteSpace(request.notes) && request.notes.StartsWith("[WALKIN]", StringComparison.OrdinalIgnoreCase))
                {
                    request.notes = request.notes.Substring("[WALKIN]".Length).Trim();
                }

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

                // The backend rejects walk-in token ranges for slots not configured for them.
                // This path always uses ONLINE token allocation and should not retry with WALKIN.
                if (rawResponse != null && rawResponse.Contains("token range not configured", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("--- RETRYING OP REGISTRATION WITH reg_type = 'ONLINE' ---");
                    request.reg_type = "ONLINE";
                    request.is_direct_walkin = false;
                    var retryResponse = await _http.PostAsJsonAsync("api/OpRegistration/create", request);
                    rawResponse = await retryResponse.Content.ReadAsStringAsync();

                    Console.WriteLine("--- RETRY OP RESPONSE ---");
                    Console.WriteLine(rawResponse);
                    Console.WriteLine("-------------------------");
                }

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

                if (!response.IsSuccessStatusCode || 
                    rawResponse.Contains("token range not configured", StringComparison.OrdinalIgnoreCase) || 
                    rawResponse.Contains("not configured on this slot", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("[DirectWalkinAsync] Token range error detected. Attempting Fallback 1: nullifying slot_detail_id...");
                    var originalSlotId = request.slot_detail_id;
                    request.slot_detail_id = null;

                    var fallbackResponse = await _http.PostAsJsonAsync("api/OpRegistration/direct-walkin", request);
                    var fallbackRaw = await fallbackResponse.Content.ReadAsStringAsync();

                    Console.WriteLine("--- FALLBACK 1 DIRECT WALKIN RESPONSE ---");
                    Console.WriteLine(fallbackRaw);
                    Console.WriteLine("------------------------------------------");

                    if (fallbackResponse.IsSuccessStatusCode && 
                        !fallbackRaw.Contains("token range not configured", StringComparison.OrdinalIgnoreCase) && 
                        !fallbackRaw.Contains("not configured on this slot", StringComparison.OrdinalIgnoreCase))
                    {
                        return fallbackRaw;
                    }

                    // Fallback 2: Pre-book appointment first then register OP
                    request.slot_detail_id = originalSlotId;
                    if (originalSlotId.HasValue && originalSlotId.Value != Guid.Empty && request.custid > 0)
                    {
                        Console.WriteLine("[DirectWalkinAsync] Fallback 1 failed. Attempting Fallback 2: Pre-booking appointment...");
                        var targetDcode = request.dcode ?? request.duty_dcode ?? 0;
                        if (targetDcode > 0)
                        {
                            var newBooking = new AppointmentBookingModel
                            {
                                booking_id = Guid.NewGuid(),
                                custid = request.custid,
                                dcode = targetDcode,
                                slot_detail_id = originalSlotId.Value,
                                appointment_date = DateOnly.FromDateTime(DateTime.UtcNow.ToIndianTime()),
                                booking_type = "ONLINE",
                                booking_status = "CONFIRMED",
                                notes = request.notes
                            };

                            var bookResult = await BookAppointmentAsync(newBooking);
                            if (bookResult != null && bookResult.Contains("SUCCESS", StringComparison.OrdinalIgnoreCase))
                            {
                                var regModel = new OPRegistrationModel.OpRegistrationModel
                                {
                                    booking_id = newBooking.booking_id,
                                    custid = request.custid,
                                    dcode = targetDcode,
                                    slot_detail_id = originalSlotId.Value,
                                    visit_date = DateOnly.FromDateTime(DateTime.UtcNow.ToIndianTime()),
                                    reg_type = "OP",
                                    visit_status = "ARRIVED",
                                    notes = request.notes
                                };

                                var opResult = await RegisterOpAsync(regModel);
                                if (!string.IsNullOrWhiteSpace(opResult) && opResult.Contains("SUCCESS", StringComparison.OrdinalIgnoreCase))
                                {
                                    return opResult;
                                }
                            }
                        }
                    }

                    return fallbackRaw;
                }

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

                if (!response.IsSuccessStatusCode || 
                    rawResponse.Contains("token range not configured", StringComparison.OrdinalIgnoreCase) || 
                    rawResponse.Contains("not configured on this slot", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("[DressingRegistrationAsync] Token range error detected. Nullifying slot_detail_id...");
                    request.slot_detail_id = null;

                    var fallbackResponse = await _http.PostAsJsonAsync("api/OpRegistration/dressing", request);
                    var fallbackRaw = await fallbackResponse.Content.ReadAsStringAsync();
                    return fallbackRaw;
                }

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

        public async Task<bool> DeleteOpVitalsAsync(Guid vitalId)
        {
            try
            {
                var response = await _http.PostAsync($"api/OpRegistration/delete-vitals?vital_id={vitalId}", null);
                if (!response.IsSuccessStatusCode)
                {
                    response = await _http.DeleteAsync($"api/OpRegistration/delete-vitals?vital_id={vitalId}");
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting OP vitals: {ex.Message}");
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
                var response = await _http.GetAsync("api/OpRegistration/all");
                if (response.IsSuccessStatusCode)
                {
                    var list = await response.Content.ReadFromJsonAsync<List<OPRegistrationModel.OpRegistrationModel>>();
                    return list ?? new List<OPRegistrationModel.OpRegistrationModel>();
                }
                Console.WriteLine($"[GetAllOpRegistrationsAsync] Non-success status: {response.StatusCode}");
                return new List<OPRegistrationModel.OpRegistrationModel>();
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
