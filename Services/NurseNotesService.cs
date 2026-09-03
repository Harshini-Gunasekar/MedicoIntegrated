using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using medico_backend.Model;
using static medico_backend.Model.IpBedsideChartModel;

namespace Booking.Services
{
    public class NurseNotesService
    {
        private readonly HttpClient _http;
        private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public NurseNotesService(HttpClient http)
        {
            _http = http;
        }

        // ══════════════════════════════════════════════════════════════════════
        // 1. TEMPERATURE
        // ══════════════════════════════════════════════════════════════════════
        public async Task<(bool Success, string Message)> AddTemperatureAsync(AddTemperatureRequest request, string? tenantCode = null)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Post, "api/IpBedsideChart/temperature/add");
                req.Content = JsonContent.Create(request);
                AddTenantHeaders(req, tenantCode);

                var response = await _http.SendAsync(req);
                var raw = await response.Content.ReadAsStringAsync();
                return (response.IsSuccessStatusCode, ParseResponseMessage(raw, response.IsSuccessStatusCode, "Temperature record added successfully."));
            }
            catch (Exception ex)
            {
                return (false, $"Error adding temperature: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> UpdateTemperatureAsync(UpdateTemperatureRequest request, string? tenantCode = null)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Post, "api/IpBedsideChart/temperature/update");
                req.Content = JsonContent.Create(request);
                AddTenantHeaders(req, tenantCode);

                var response = await _http.SendAsync(req);
                var raw = await response.Content.ReadAsStringAsync();
                return (response.IsSuccessStatusCode, ParseResponseMessage(raw, response.IsSuccessStatusCode, "Temperature record updated successfully."));
            }
            catch (Exception ex)
            {
                return (false, $"Error updating temperature: {ex.Message}");
            }
        }

        public async Task<List<NurseTemperatureModel>> GetTemperaturesAsync(Guid ipId, string? tenantCode = null)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, $"api/IpBedsideChart/temperature/{ipId}");
                AddTenantHeaders(req, tenantCode);

                var response = await _http.SendAsync(req);
                if (!response.IsSuccessStatusCode) return new();

                var raw = await response.Content.ReadAsStringAsync();
                return DeserializeList<NurseTemperatureModel>(raw);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NurseNotesService] GetTemperaturesAsync error: {ex.Message}");
                return new();
            }
        }

        public async Task<(bool Success, string Message)> DeleteTemperatureAsync(Guid tempId, string? tenantCode = null)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, $"api/IpBedsideChart/temperature/delete?temp_id={tempId}");
                AddTenantHeaders(req, tenantCode);

                var response = await _http.SendAsync(req);
                var raw = await response.Content.ReadAsStringAsync();
                return (response.IsSuccessStatusCode, ParseResponseMessage(raw, response.IsSuccessStatusCode, "Temperature record deleted."));
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting temperature: {ex.Message}");
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // 2. INPUT / OUTPUT
        // ══════════════════════════════════════════════════════════════════════
        public async Task<(bool Success, string Message)> AddInputOutputAsync(AddInputOutputRequest request, string? tenantCode = null)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Post, "api/IpBedsideChart/input-output/add");
                req.Content = JsonContent.Create(request);
                AddTenantHeaders(req, tenantCode);

                var response = await _http.SendAsync(req);
                var raw = await response.Content.ReadAsStringAsync();
                return (response.IsSuccessStatusCode, ParseResponseMessage(raw, response.IsSuccessStatusCode, "Input/Output record added."));
            }
            catch (Exception ex)
            {
                return (false, $"Error adding input/output: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> UpdateInputOutputAsync(UpdateInputOutputRequest request, string? tenantCode = null)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Post, "api/IpBedsideChart/input-output/update");
                req.Content = JsonContent.Create(request);
                AddTenantHeaders(req, tenantCode);

                var response = await _http.SendAsync(req);
                var raw = await response.Content.ReadAsStringAsync();
                return (response.IsSuccessStatusCode, ParseResponseMessage(raw, response.IsSuccessStatusCode, "Input/Output record updated."));
            }
            catch (Exception ex)
            {
                return (false, $"Error updating input/output: {ex.Message}");
            }
        }

        public async Task<List<NurseInputOutputModel>> GetInputOutputsAsync(Guid ipId, string? tenantCode = null)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, $"api/IpBedsideChart/input-output/{ipId}");
                AddTenantHeaders(req, tenantCode);

                var response = await _http.SendAsync(req);
                if (!response.IsSuccessStatusCode) return new();

                var raw = await response.Content.ReadAsStringAsync();
                return DeserializeList<NurseInputOutputModel>(raw);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NurseNotesService] GetInputOutputsAsync error: {ex.Message}");
                return new();
            }
        }

        public async Task<(bool Success, string Message)> DeleteInputOutputAsync(Guid ioId, string? tenantCode = null)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, $"api/IpBedsideChart/input-output/delete?io_id={ioId}");
                AddTenantHeaders(req, tenantCode);

                var response = await _http.SendAsync(req);
                var raw = await response.Content.ReadAsStringAsync();
                return (response.IsSuccessStatusCode, ParseResponseMessage(raw, response.IsSuccessStatusCode, "Input/Output record deleted."));
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting input/output: {ex.Message}");
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // 3. SERVICE
        // ══════════════════════════════════════════════════════════════════════
        public async Task<(bool Success, string Message)> AddServiceAsync(AddServiceRequest request, string? tenantCode = null)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Post, "api/IpBedsideChart/service/add");
                req.Content = JsonContent.Create(request);
                AddTenantHeaders(req, tenantCode);

                var response = await _http.SendAsync(req);
                var raw = await response.Content.ReadAsStringAsync();
                return (response.IsSuccessStatusCode, ParseResponseMessage(raw, response.IsSuccessStatusCode, "Service record added."));
            }
            catch (Exception ex)
            {
                return (false, $"Error adding service: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> UpdateServiceAsync(UpdateServiceRequest request, string? tenantCode = null)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Post, "api/IpBedsideChart/service/update");
                req.Content = JsonContent.Create(request);
                AddTenantHeaders(req, tenantCode);

                var response = await _http.SendAsync(req);
                var raw = await response.Content.ReadAsStringAsync();
                return (response.IsSuccessStatusCode, ParseResponseMessage(raw, response.IsSuccessStatusCode, "Service record updated."));
            }
            catch (Exception ex)
            {
                return (false, $"Error updating service: {ex.Message}");
            }
        }

        public async Task<List<NurseServiceModel>> GetServicesAsync(Guid ipId, string? tenantCode = null)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, $"api/IpBedsideChart/service/{ipId}");
                AddTenantHeaders(req, tenantCode);

                var response = await _http.SendAsync(req);
                if (!response.IsSuccessStatusCode) return new();

                var raw = await response.Content.ReadAsStringAsync();
                return DeserializeList<NurseServiceModel>(raw);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NurseNotesService] GetServicesAsync error: {ex.Message}");
                return new();
            }
        }

        public async Task<(bool Success, string Message)> DeleteServiceAsync(Guid serviceId, string? tenantCode = null)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, $"api/IpBedsideChart/service/delete?service_id={serviceId}");
                AddTenantHeaders(req, tenantCode);

                var response = await _http.SendAsync(req);
                var raw = await response.Content.ReadAsStringAsync();
                return (response.IsSuccessStatusCode, ParseResponseMessage(raw, response.IsSuccessStatusCode, "Service record deleted."));
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting service: {ex.Message}");
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // 4. MEDICINE
        // ══════════════════════════════════════════════════════════════════════
        public async Task<(bool Success, string Message)> AddMedicineAsync(AddMedicineRequest request, string? tenantCode = null)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Post, "api/IpBedsideChart/medicine/add");
                req.Content = JsonContent.Create(request);
                AddTenantHeaders(req, tenantCode);

                var response = await _http.SendAsync(req);
                var raw = await response.Content.ReadAsStringAsync();
                return (response.IsSuccessStatusCode, ParseResponseMessage(raw, response.IsSuccessStatusCode, "Medicine record added."));
            }
            catch (Exception ex)
            {
                return (false, $"Error adding medicine: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> UpdateMedicineAsync(UpdateMedicineRequest request, string? tenantCode = null)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Post, "api/IpBedsideChart/medicine/update");
                req.Content = JsonContent.Create(request);
                AddTenantHeaders(req, tenantCode);

                var response = await _http.SendAsync(req);
                var raw = await response.Content.ReadAsStringAsync();
                return (response.IsSuccessStatusCode, ParseResponseMessage(raw, response.IsSuccessStatusCode, "Medicine record updated."));
            }
            catch (Exception ex)
            {
                return (false, $"Error updating medicine: {ex.Message}");
            }
        }

        public async Task<List<NurseMedicineModel>> GetMedicinesAsync(Guid ipId, string? tenantCode = null)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, $"api/IpBedsideChart/medicine/{ipId}");
                AddTenantHeaders(req, tenantCode);

                var response = await _http.SendAsync(req);
                if (!response.IsSuccessStatusCode) return new();

                var raw = await response.Content.ReadAsStringAsync();
                return DeserializeList<NurseMedicineModel>(raw);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NurseNotesService] GetMedicinesAsync error: {ex.Message}");
                return new();
            }
        }

        public async Task<(bool Success, string Message)> DeleteMedicineAsync(Guid medId, string? tenantCode = null)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, $"api/IpBedsideChart/medicine/delete?med_id={medId}");
                AddTenantHeaders(req, tenantCode);

                var response = await _http.SendAsync(req);
                var raw = await response.Content.ReadAsStringAsync();
                return (response.IsSuccessStatusCode, ParseResponseMessage(raw, response.IsSuccessStatusCode, "Medicine record deleted."));
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting medicine: {ex.Message}");
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // 5. OTHER
        // ══════════════════════════════════════════════════════════════════════
        public async Task<(bool Success, string Message)> AddOtherAsync(AddOtherRequest request, string? tenantCode = null)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Post, "api/IpBedsideChart/other/add");
                req.Content = JsonContent.Create(request);
                AddTenantHeaders(req, tenantCode);

                var response = await _http.SendAsync(req);
                var raw = await response.Content.ReadAsStringAsync();
                return (response.IsSuccessStatusCode, ParseResponseMessage(raw, response.IsSuccessStatusCode, "Note added."));
            }
            catch (Exception ex)
            {
                return (false, $"Error adding note: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> UpdateOtherAsync(UpdateOtherRequest request, string? tenantCode = null)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Post, "api/IpBedsideChart/other/update");
                req.Content = JsonContent.Create(request);
                AddTenantHeaders(req, tenantCode);

                var response = await _http.SendAsync(req);
                var raw = await response.Content.ReadAsStringAsync();
                return (response.IsSuccessStatusCode, ParseResponseMessage(raw, response.IsSuccessStatusCode, "Note updated."));
            }
            catch (Exception ex)
            {
                return (false, $"Error updating note: {ex.Message}");
            }
        }

        public async Task<List<NurseOtherModel>> GetOthersAsync(Guid ipId, string? tenantCode = null)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, $"api/IpBedsideChart/other/{ipId}");
                AddTenantHeaders(req, tenantCode);

                var response = await _http.SendAsync(req);
                if (!response.IsSuccessStatusCode) return new();

                var raw = await response.Content.ReadAsStringAsync();
                return DeserializeList<NurseOtherModel>(raw);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NurseNotesService] GetOthersAsync error: {ex.Message}");
                return new();
            }
        }

        public async Task<(bool Success, string Message)> DeleteOtherAsync(Guid otherId, string? tenantCode = null)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, $"api/IpBedsideChart/other/delete?other_id={otherId}");
                AddTenantHeaders(req, tenantCode);

                var response = await _http.SendAsync(req);
                var raw = await response.Content.ReadAsStringAsync();
                return (response.IsSuccessStatusCode, ParseResponseMessage(raw, response.IsSuccessStatusCode, "Note deleted."));
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting note: {ex.Message}");
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // 6. VISITING DOCTOR
        // ══════════════════════════════════════════════════════════════════════
        public async Task<(bool Success, string Message)> AddVisitingDoctorAsync(AddVisitingDoctorRequest request, string? tenantCode = null, int? userCode = null)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Post, "api/IpBedsideChart/visiting-doctor/add");
                req.Content = JsonContent.Create(request);
                AddTenantHeaders(req, tenantCode, userCode);

                var response = await _http.SendAsync(req);
                var raw = await response.Content.ReadAsStringAsync();
                return (response.IsSuccessStatusCode, ParseResponseMessage(raw, response.IsSuccessStatusCode, "Visiting doctor entry recorded successfully."));
            }
            catch (Exception ex)
            {
                return (false, $"Error adding visiting doctor: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> UpdateVisitingDoctorAsync(UpdateVisitingDoctorRequest request, string? tenantCode = null, int? userCode = null)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Post, "api/IpBedsideChart/visiting-doctor/update");
                req.Content = JsonContent.Create(request);
                AddTenantHeaders(req, tenantCode, userCode);

                var response = await _http.SendAsync(req);
                var raw = await response.Content.ReadAsStringAsync();
                return (response.IsSuccessStatusCode, ParseResponseMessage(raw, response.IsSuccessStatusCode, "Visiting doctor entry updated successfully."));
            }
            catch (Exception ex)
            {
                return (false, $"Error updating visiting doctor: {ex.Message}");
            }
        }

        public async Task<List<NurseVisitingDoctorModel>> GetVisitingDoctorsAsync(Guid ipId, string? tenantCode = null, int? userCode = null)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, $"api/IpBedsideChart/visiting-doctor/{ipId}");
                AddTenantHeaders(req, tenantCode, userCode);

                var response = await _http.SendAsync(req);
                if (!response.IsSuccessStatusCode) return new();

                var raw = await response.Content.ReadAsStringAsync();
                return DeserializeList<NurseVisitingDoctorModel>(raw);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NurseNotesService] GetVisitingDoctorsAsync error: {ex.Message}");
                return new();
            }
        }

        public async Task<NurseVisitingDoctorModel?> GetVisitingDoctorDetailAsync(Guid ipId, Guid visitId, string? tenantCode = null, int? userCode = null)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, $"api/IpBedsideChart/visiting-doctor/{ipId}/detail?visit_id={visitId}");
                AddTenantHeaders(req, tenantCode, userCode);

                var response = await _http.SendAsync(req);
                if (!response.IsSuccessStatusCode) return null;

                var raw = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(raw)) return null;

                return JsonSerializer.Deserialize<NurseVisitingDoctorModel>(raw, _jsonOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NurseNotesService] GetVisitingDoctorDetailAsync error: {ex.Message}");
                return null;
            }
        }

        public async Task<(bool Success, string Message)> DeleteVisitingDoctorAsync(Guid visitId, string? tenantCode = null, int? userCode = null)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, $"api/IpBedsideChart/visiting-doctor/delete?visit_id={visitId}");
                AddTenantHeaders(req, tenantCode, userCode);

                var response = await _http.SendAsync(req);
                var raw = await response.Content.ReadAsStringAsync();
                return (response.IsSuccessStatusCode, ParseResponseMessage(raw, response.IsSuccessStatusCode, "Visiting doctor entry deleted."));
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting visiting doctor entry: {ex.Message}");
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // 7. SCHEDULE
        // ══════════════════════════════════════════════════════════════════════
        public async Task<(bool Success, string Message)> AddScheduleAsync(AddScheduleRequest request, string? tenantCode = null, int? userCode = null)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Post, "api/IpBedsideChart/schedule/add");
                req.Content = JsonContent.Create(request);
                AddTenantHeaders(req, tenantCode, userCode);

                var response = await _http.SendAsync(req);
                var raw = await response.Content.ReadAsStringAsync();
                return (response.IsSuccessStatusCode, ParseResponseMessage(raw, response.IsSuccessStatusCode, "Schedule item added successfully."));
            }
            catch (Exception ex)
            {
                return (false, $"Error adding schedule: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> UpdateScheduleAsync(UpdateScheduleRequest request, string? tenantCode = null, int? userCode = null)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Post, "api/IpBedsideChart/schedule/update");
                req.Content = JsonContent.Create(request);
                AddTenantHeaders(req, tenantCode, userCode);

                var response = await _http.SendAsync(req);
                var raw = await response.Content.ReadAsStringAsync();
                return (response.IsSuccessStatusCode, ParseResponseMessage(raw, response.IsSuccessStatusCode, "Schedule item updated successfully."));
            }
            catch (Exception ex)
            {
                return (false, $"Error updating schedule: {ex.Message}");
            }
        }

        public async Task<List<NurseScheduleModel>> GetSchedulesAsync(Guid ipId, string? tenantCode = null, int? userCode = null)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, $"api/IpBedsideChart/schedule/{ipId}");
                AddTenantHeaders(req, tenantCode, userCode);

                var response = await _http.SendAsync(req);
                if (!response.IsSuccessStatusCode) return new();

                var raw = await response.Content.ReadAsStringAsync();
                return DeserializeList<NurseScheduleModel>(raw);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NurseNotesService] GetSchedulesAsync error: {ex.Message}");
                return new();
            }
        }

        public async Task<NurseScheduleModel?> GetScheduleDetailAsync(Guid ipId, Guid scheduleId, string? tenantCode = null, int? userCode = null)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, $"api/IpBedsideChart/schedule/{ipId}/detail?schedule_id={scheduleId}");
                AddTenantHeaders(req, tenantCode, userCode);

                var response = await _http.SendAsync(req);
                if (!response.IsSuccessStatusCode) return null;

                var raw = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(raw)) return null;

                return JsonSerializer.Deserialize<NurseScheduleModel>(raw, _jsonOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NurseNotesService] GetScheduleDetailAsync error: {ex.Message}");
                return null;
            }
        }

        public async Task<(bool Success, string Message)> DeleteScheduleAsync(Guid scheduleId, string? tenantCode = null, int? userCode = null)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, $"api/IpBedsideChart/schedule/delete?schedule_id={scheduleId}");
                AddTenantHeaders(req, tenantCode, userCode);

                var response = await _http.SendAsync(req);
                var raw = await response.Content.ReadAsStringAsync();
                return (response.IsSuccessStatusCode, ParseResponseMessage(raw, response.IsSuccessStatusCode, "Schedule item deleted."));
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting schedule: {ex.Message}");
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // 8. COMBINED SUMMARY
        // ══════════════════════════════════════════════════════════════════════
        public async Task<NurseNotesSummaryModel> GetSummaryAsync(Guid ipId, string? tenantCode = null, int? userCode = null)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, $"api/IpBedsideChart/summary/{ipId}");
                AddTenantHeaders(req, tenantCode, userCode);

                var response = await _http.SendAsync(req);
                if (!response.IsSuccessStatusCode) return new();

                var raw = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(raw)) return new();

                return JsonSerializer.Deserialize<NurseNotesSummaryModel>(raw, _jsonOptions) ?? new();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NurseNotesService] GetSummaryAsync error: {ex.Message}");
                return new();
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // HELPERS
        // ══════════════════════════════════════════════════════════════════════
        public static string FormatTimeForApi(string? timeInput, string fallbackTime = "08:00:00")
        {
            if (string.IsNullOrWhiteSpace(timeInput)) return fallbackTime;
            var trimmed = timeInput.Trim();

            // 1. If it's already HH:mm:ss
            if (System.Text.RegularExpressions.Regex.IsMatch(trimmed, @"^\d{1,2}:\d{2}:\d{2}$"))
            {
                if (TimeSpan.TryParse(trimmed, out var ts1))
                    return $"{(int)ts1.TotalHours:D2}:{ts1.Minutes:D2}:{ts1.Seconds:D2}";
                return trimmed;
            }

            // 2. If it's HH:mm (e.g. from <input type="time">)
            if (System.Text.RegularExpressions.Regex.IsMatch(trimmed, @"^\d{1,2}:\d{2}$"))
            {
                if (TimeSpan.TryParse(trimmed, out var ts2))
                    return $"{(int)ts2.TotalHours:D2}:{ts2.Minutes:D2}:00";
                return $"{trimmed}:00";
            }

            // 3. If it has AM/PM (e.g. "10:30 AM", "02:45 PM")
            if (DateTime.TryParse(trimmed, out var dt))
            {
                return dt.ToString("HH:mm:ss");
            }

            if (TimeOnly.TryParse(trimmed, out var tOnly))
            {
                return tOnly.ToString("HH:mm:ss");
            }

            return fallbackTime;
        }

        private void AddTenantHeaders(HttpRequestMessage req, string? tenantCode, int? userCode = null)
        {
            if (!string.IsNullOrWhiteSpace(tenantCode))
            {
                req.Headers.Remove("tenant_code");
                req.Headers.Remove("tenantcode");
                req.Headers.Remove("tenant-code");
                req.Headers.Add("tenant_code", tenantCode);
                req.Headers.Add("tenantcode", tenantCode);
                req.Headers.Add("tenant-code", tenantCode);
            }
            if (userCode.HasValue && userCode.Value > 0)
            {
                req.Headers.Remove("usercode");
                req.Headers.Remove("user_code");
                req.Headers.Add("usercode", userCode.Value.ToString());
                req.Headers.Add("user_code", userCode.Value.ToString());
            }
        }

        private string ParseResponseMessage(string raw, bool isSuccess, string defaultSuccess)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return isSuccess ? defaultSuccess : "Request failed.";

            var trimmed = raw.Trim().Trim('"');
            try
            {
                using var doc = JsonDocument.Parse(raw);
                if (doc.RootElement.TryGetProperty("message", out var mProp))
                    return mProp.GetString() ?? trimmed;

                if (doc.RootElement.TryGetProperty("errors", out var errorsProp) && errorsProp.ValueKind == JsonValueKind.Object)
                {
                    var errList = new List<string>();
                    foreach (var prop in errorsProp.EnumerateObject())
                    {
                        if (prop.Value.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var item in prop.Value.EnumerateArray())
                            {
                                if (item.GetString() is string s && !string.IsNullOrWhiteSpace(s))
                                    errList.Add(s);
                            }
                        }
                    }
                    if (errList.Count > 0) return string.Join(" | ", errList);
                }

                if (doc.RootElement.TryGetProperty("title", out var titleProp))
                    return titleProp.GetString() ?? trimmed;
            }
            catch { }

            return trimmed;
        }

        private List<T> DeserializeList<T>(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return new();
            try
            {
                using var doc = JsonDocument.Parse(raw);
                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    return JsonSerializer.Deserialize<List<T>>(raw, _jsonOptions) ?? new();
                }
                if (doc.RootElement.TryGetProperty("value", out var val) && val.ValueKind == JsonValueKind.Array)
                {
                    return JsonSerializer.Deserialize<List<T>>(val.GetRawText(), _jsonOptions) ?? new();
                }
                if (doc.RootElement.TryGetProperty("data", out var dataVal) && dataVal.ValueKind == JsonValueKind.Array)
                {
                    return JsonSerializer.Deserialize<List<T>>(dataVal.GetRawText(), _jsonOptions) ?? new();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NurseNotesService] DeserializeList error for {typeof(T).Name}: {ex.Message}");
            }
            return new();
        }
    }
}
