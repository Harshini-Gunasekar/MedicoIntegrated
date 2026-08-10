using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using medico_backend.Model;

namespace Booking.Services
{
    public class NurseNotesService
    {
        private readonly HttpClient _http;

        public NurseNotesService(HttpClient http)
        {
            _http = http;
        }

        // ─── ADD ────────────────────────────────────────────────────────────
        public async Task<(bool Success, string Message)> AddNurseNoteAsync(AddNurseNoteRequest request)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/NurseNotes/add", request);
                var raw = await response.Content.ReadAsStringAsync();
                var msg = raw.Trim('"').Trim();
                return (response.IsSuccessStatusCode, string.IsNullOrWhiteSpace(msg) ? (response.IsSuccessStatusCode ? "Note added." : "Failed to add note.") : msg);
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        // ─── GET BY IP ──────────────────────────────────────────────────────
        public async Task<List<NurseNotesModel>> GetNotesByIpAsync(Guid ipId, string? noteType = null)
        {
            try
            {
                var url = $"api/NurseNotes/by-ip/{ipId}";
                if (!string.IsNullOrWhiteSpace(noteType))
                    url += $"?note_type={noteType}";

                var response = await _http.GetAsync(url);
                if (!response.IsSuccessStatusCode) return new();
                var raw = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(raw)) return new();

                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                using var doc = System.Text.Json.JsonDocument.Parse(raw);
                if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                    return System.Text.Json.JsonSerializer.Deserialize<List<NurseNotesModel>>(raw, options) ?? new();

                if (doc.RootElement.TryGetProperty("value", out var val) && val.ValueKind == System.Text.Json.JsonValueKind.Array)
                    return System.Text.Json.JsonSerializer.Deserialize<List<NurseNotesModel>>(val.GetRawText(), options) ?? new();

                return new();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetNotesByIpAsync error: {ex.Message}");
                return new();
            }
        }

        // ─── MEDICATION HISTORY ─────────────────────────────────────────────
        public async Task<List<NurseNotesModel>> GetMedicationHistoryAsync(Guid ipId)
        {
            try
            {
                var response = await _http.GetAsync($"api/NurseNotes/medication-history/{ipId}");
                if (!response.IsSuccessStatusCode) return new();
                var raw = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(raw)) return new();
                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                using var doc = System.Text.Json.JsonDocument.Parse(raw);
                if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                    return System.Text.Json.JsonSerializer.Deserialize<List<NurseNotesModel>>(raw, options) ?? new();
                if (doc.RootElement.TryGetProperty("value", out var val) && val.ValueKind == System.Text.Json.JsonValueKind.Array)
                    return System.Text.Json.JsonSerializer.Deserialize<List<NurseNotesModel>>(val.GetRawText(), options) ?? new();
                return new();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetMedicationHistoryAsync error: {ex.Message}");
                return new();
            }
        }

        // ─── FULL CHART ─────────────────────────────────────────────────────
        public async Task<IpNursingChartViewModel?> GetFullChartAsync(Guid ipId)
        {
            try
            {
                var response = await _http.GetAsync($"api/NurseNotes/full-chart/{ipId}");
                if (!response.IsSuccessStatusCode) return null;
                var raw = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(raw)) return null;
                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return System.Text.Json.JsonSerializer.Deserialize<IpNursingChartViewModel>(raw, options);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetFullChartAsync error: {ex.Message}");
                return null;
            }
        }

        // ─── UPDATE ─────────────────────────────────────────────────────────
        public async Task<(bool Success, string Message)> UpdateNurseNoteAsync(UpdateNurseNoteRequest request)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/NurseNotes/update", request);
                var raw = await response.Content.ReadAsStringAsync();
                var msg = raw.Trim('"').Trim();
                return (response.IsSuccessStatusCode, string.IsNullOrWhiteSpace(msg) ? (response.IsSuccessStatusCode ? "Note updated." : "Failed to update note.") : msg);
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        // ─── VERIFY ─────────────────────────────────────────────────────────
        public async Task<(bool Success, string Message)> VerifyNurseNoteAsync(VerifyNurseNoteRequest request)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/NurseNotes/verify", request);
                var raw = await response.Content.ReadAsStringAsync();
                var msg = raw.Trim('"').Trim();
                return (response.IsSuccessStatusCode, string.IsNullOrWhiteSpace(msg) ? (response.IsSuccessStatusCode ? "Note verified." : "Failed to verify note.") : msg);
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        // ─── CANCEL ─────────────────────────────────────────────────────────
        public async Task<(bool Success, string Message)> CancelNurseNoteAsync(CancelNurseNoteRequest request)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/NurseNotes/cancel", request);
                var raw = await response.Content.ReadAsStringAsync();
                var msg = raw.Trim('"').Trim();
                return (response.IsSuccessStatusCode, string.IsNullOrWhiteSpace(msg) ? (response.IsSuccessStatusCode ? "Note cancelled." : "Failed to cancel note.") : msg);
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        // ─── DELETE ─────────────────────────────────────────────────────────
        public async Task<(bool Success, string Message)> DeleteNurseNoteAsync(Guid noteId)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/NurseNotes/delete/{noteId}");
                var raw = await response.Content.ReadAsStringAsync();
                var msg = raw.Trim('"').Trim();
                return (response.IsSuccessStatusCode, string.IsNullOrWhiteSpace(msg) ? (response.IsSuccessStatusCode ? "Note deleted." : "Failed to delete note.") : msg);
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }
    }
}
