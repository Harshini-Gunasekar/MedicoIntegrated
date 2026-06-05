using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Booking.Models;

namespace Booking.Services
{
    public class CaseSheetService
    {
        private readonly HttpClient _http;

        public CaseSheetService(HttpClient http)
        {
            _http = http;
        }

        public async Task<string> SaveCaseSheetAsync(SaveCaseSheetRequest request)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/CaseSheet/save", request);
                var rawResponse = await response.Content.ReadAsStringAsync();
                return rawResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving case sheet: {ex.Message}");
                return $"Error|{ex.Message}";
            }
        }

        public async Task<bool> FinalizeCaseSheetAsync(FinalizeCaseSheetRequest request)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/CaseSheet/finalize", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error finalizing case sheet: {ex.Message}");
                return false;
            }
        }

        public async Task<CaseSheetViewModel?> GetCaseSheetByVisitAsync(Guid opId)
        {
            try
            {
                return await _http.GetFromJsonAsync<CaseSheetViewModel>($"api/CaseSheet/by-visit?op_id={opId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting case sheet by visit: {ex.Message}");
                return null;
            }
        }

        public async Task<CaseSheetPrescriptionViewModel?> GetPrescriptionByVisitAsync(Guid opId)
        {
            try
            {
                return await _http.GetFromJsonAsync<CaseSheetPrescriptionViewModel>($"api/CaseSheet/prescription?op_id={opId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting prescription by visit: {ex.Message}");
                return null;
            }
        }

        public async Task<List<CaseSheetViewModel>> GetPatientHistoryAsync(decimal custid, int pageNo = 1, int pageSize = 10)
        {
            try
            {
                // The history endpoint might return a list of view models directly or a paginated wrapper.
                // We'll attempt to deserialize as a list, and if that fails, try a paginated wrapper or return empty.
                var response = await _http.GetAsync($"api/CaseSheet/history?custid={custid}&pageNo={pageNo}&pageSize={pageSize}");
                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var list = await response.Content.ReadFromJsonAsync<List<CaseSheetViewModel>>();
                        return list ?? new List<CaseSheetViewModel>();
                    }
                    catch
                    {
                        // Fallback in case of wrapper
                        var wrapper = await response.Content.ReadFromJsonAsync<HistoryResponseWrapper>();
                        return wrapper?.items ?? new List<CaseSheetViewModel>();
                    }
                }
                return new List<CaseSheetViewModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting patient history: {ex.Message}");
                return new List<CaseSheetViewModel>();
            }
        }

        public async Task<List<IcdSearchResult>> SearchIcd10Async(string query, int limit = 10)
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<IcdSearchResult>>($"api/CaseSheet/icd-search?query={Uri.EscapeDataString(query)}&limit={limit}");
                return response ?? new List<IcdSearchResult>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching ICD-10 codes: {ex.Message}");
                return new List<IcdSearchResult>();
            }
        }

        public async Task<bool> UpdateInvestigationResultAsync(UpdateInvestigationResultRequest request)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/CaseSheet/investigation/result", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating investigation result: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteInvestigationAsync(Guid invId)
        {
            try
            {
                // Try GET first as the URL format includes a query parameter
                var response = await _http.GetAsync($"api/CaseSheet/investigation/delete?inv_id={invId}");
                if (!response.IsSuccessStatusCode)
                {
                    // Fallback to DELETE verb
                    var request = new HttpRequestMessage(HttpMethod.Delete, $"api/CaseSheet/investigation/delete?inv_id={invId}");
                    response = await _http.SendAsync(request);
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting investigation: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeletePrescriptionAsync(string prCode)
        {
            try
            {
                // Try GET first
                var response = await _http.GetAsync($"api/CaseSheet/prescription/delete?pr_code={Uri.EscapeDataString(prCode)}");
                if (!response.IsSuccessStatusCode)
                {
                    // Fallback to DELETE verb
                    var request = new HttpRequestMessage(HttpMethod.Delete, $"api/CaseSheet/prescription/delete?pr_code={Uri.EscapeDataString(prCode)}");
                    response = await _http.SendAsync(request);
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting prescription: {ex.Message}");
                return false;
            }
        }

        // Helper class to deserialize paginated histories if wrapped
        private class HistoryResponseWrapper
        {
            public List<CaseSheetViewModel> items { get; set; } = new();
            public int totalCount { get; set; }
        }
    }
}
