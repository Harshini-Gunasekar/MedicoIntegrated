using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Medico_Backend.Model;

namespace Booking.Services
{
    public class VitalsService
    {
        private readonly HttpClient _http;

        public VitalsService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<VitalsModel>> GetVitalsAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<VitalsModel>>("api/Vitals/get");
                return response ?? new List<VitalsModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting vitals: {ex.Message}");
                return new List<VitalsModel>();
            }
        }

        public async Task<VitalsModel?> GetVitalByIdAsync(int vitalentryid)
        {
            try
            {
                return await _http.GetFromJsonAsync<VitalsModel>($"api/Vitals/get-by-id?vitalentryid={vitalentryid}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting vital by id {vitalentryid}: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> InsertVitalAsync(VitalsModel vital)
        {
            try
            {
                var jsonPayload = JsonSerializer.Serialize(vital, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine("\n==========================================");
                Console.WriteLine("[POST /api/vitals/insert] Payload:");
                Console.WriteLine(jsonPayload);
                Console.WriteLine("==========================================\n");

                var response = await _http.PostAsJsonAsync("api/vitals/insert", vital);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting vital entry: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateVitalAsync(VitalsModel vital)
        {
            try
            {
                var jsonPayload = JsonSerializer.Serialize(vital, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine("\n==========================================");
                Console.WriteLine("[POST /api/Vitals/update] Payload:");
                Console.WriteLine(jsonPayload);
                Console.WriteLine("==========================================\n");

                var response = await _http.PostAsJsonAsync("api/Vitals/update", vital);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating vital entry: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateVitalStatusAsync(UpdateVitalStatusRequest request)
        {
            try
            {
                var jsonPayload = JsonSerializer.Serialize(request, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine("\n==========================================");
                Console.WriteLine("[POST /api/Vitals/update-status] Payload:");
                Console.WriteLine(jsonPayload);
                Console.WriteLine("==========================================\n");

                var response = await _http.PostAsJsonAsync("api/Vitals/update-status", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating vital status: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteVitalAsync(int vitalentryid)
        {
            try
            {
                Console.WriteLine($"\n[GET /api/Vitals/delete] vitalentryid={vitalentryid}\n");
                var response = await _http.GetAsync($"api/Vitals/delete?vitalentryid={vitalentryid}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting vital entry {vitalentryid}: {ex.Message}");
                return false;
            }
        }

        // --- Lab Result Entry ---

        public async Task<List<LabResultEntryModel>> GetLabResultEntriesAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<LabResultEntryModel>>("api/lab-result-entry/get");
                return response ?? new List<LabResultEntryModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting lab result entries: {ex.Message}");
                return new List<LabResultEntryModel>();
            }
        }

        public async Task<List<LabResultEntryModel>> SearchLabResultEntriesAsync(string? name, DateTime? date = null)
        {
            try
            {
                string url = "api/lab-result-entry/search";
                var queryParams = new List<string>();

                if (!string.IsNullOrWhiteSpace(name))
                {
                    queryParams.Add($"name={Uri.EscapeDataString(name.Trim())}");
                }

                if (date.HasValue && date.Value != DateTime.MinValue)
                {
                    queryParams.Add($"date={date.Value:yyyy-MM-dd}");
                }

                if (queryParams.Count > 0)
                {
                    url += "?" + string.Join("&", queryParams);
                }

                var response = await _http.GetFromJsonAsync<List<LabResultEntryModel>>(url);
                return response ?? new List<LabResultEntryModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching lab result entries: {ex.Message}");
                return new List<LabResultEntryModel>();
            }
        }

        public async Task<bool> UpdateLabResultStatusAsync(LabStatusUpdateRequest request)
        {
            try
            {
                var jsonPayload = JsonSerializer.Serialize(request, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine("\n==========================================");
                Console.WriteLine("[POST /api/lab-result-entry/update-status] Payload:");
                Console.WriteLine(jsonPayload);
                Console.WriteLine("==========================================\n");

                var response = await _http.PostAsJsonAsync("api/lab-result-entry/update-status", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating lab result status: {ex.Message}");
                return false;
            }
        }

        // --- Scan Result Entry ---

        public async Task<List<ScanResultEntryModel>> GetScanResultEntriesAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<ScanResultEntryModel>>("api/scan-result-entry/get");
                return response ?? new List<ScanResultEntryModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting scan result entries: {ex.Message}");
                return new List<ScanResultEntryModel>();
            }
        }

        public async Task<List<ScanResultEntryModel>> SearchScanResultEntriesAsync(string? name, DateTime? date = null)
        {
            try
            {
                string url = "api/scan-result-entry/search";
                var queryParams = new List<string>();

                if (!string.IsNullOrWhiteSpace(name))
                {
                    queryParams.Add($"name={Uri.EscapeDataString(name.Trim())}");
                }

                if (date.HasValue && date.Value != DateTime.MinValue)
                {
                    queryParams.Add($"date={date.Value:yyyy-MM-dd}");
                }

                if (queryParams.Count > 0)
                {
                    url += "?" + string.Join("&", queryParams);
                }

                var response = await _http.GetFromJsonAsync<List<ScanResultEntryModel>>(url);
                return response ?? new List<ScanResultEntryModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching scan result entries: {ex.Message}");
                return new List<ScanResultEntryModel>();
            }
        }

        public async Task<bool> UpdateScanResultStatusAsync(ScanStatusUpdateRequest request)
        {
            try
            {
                var jsonPayload = JsonSerializer.Serialize(request, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine("\n==========================================");
                Console.WriteLine("[POST /api/scan-result-entry/update-status] Payload:");
                Console.WriteLine(jsonPayload);
                Console.WriteLine("==========================================\n");

                var response = await _http.PostAsJsonAsync("api/scan-result-entry/update-status", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating scan result status: {ex.Message}");
                return false;
            }
        }

        // --- Update Slot Status ---
        public async Task<bool> UpdateSlotStatusAsync(UpdateVitalSlotStatusRequest request)
        {
            try
            {
                var jsonPayload = JsonSerializer.Serialize(request, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine("\n==========================================");
                Console.WriteLine("[POST /api/Vitals/update-slot-status] Payload:");
                Console.WriteLine(jsonPayload);
                Console.WriteLine("==========================================\n");

                var response = await _http.PostAsJsonAsync("api/Vitals/update-slot-status", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating vital slot status: {ex.Message}");
                return false;
            }
        }
    }
}
