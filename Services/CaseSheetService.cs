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
            var originalFollowUpDate = request.followup_date;
            try
            {
                if (request.followup_date.HasValue)
                {
                    var istZone = GetIstTimeZone();
                    var istTime = DateTime.SpecifyKind(request.followup_date.Value, DateTimeKind.Unspecified);
                    request.followup_date = TimeZoneInfo.ConvertTimeToUtc(istTime, istZone);
                }

                var jsonOptions = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                var jsonPayload = System.Text.Json.JsonSerializer.Serialize(request, jsonOptions);
                Console.WriteLine("--- SAVE CASE SHEET PAYLOAD ---");
                Console.WriteLine(jsonPayload);
                Console.WriteLine("-------------------------------");

                var response = await _http.PostAsJsonAsync("api/CaseSheet/save", request);
                var rawResponse = await response.Content.ReadAsStringAsync();

                Console.WriteLine("--- SAVE CASE SHEET RESPONSE ---");
                Console.WriteLine(rawResponse);
                Console.WriteLine("--------------------------------");

                return rawResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving case sheet: {ex.Message}");
                return $"Error|{ex.Message}";
            }
            finally
            {
                request.followup_date = originalFollowUpDate;
            }
        }

        public async Task<bool> FinalizeCaseSheetAsync(FinalizeCaseSheetRequest request)
        {
            try
            {
                var jsonOptions = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                var jsonPayload = System.Text.Json.JsonSerializer.Serialize(request, jsonOptions);
                Console.WriteLine("--- FINALIZE CASE SHEET PAYLOAD ---");
                Console.WriteLine(jsonPayload);
                Console.WriteLine("------------------------------------");

                var response = await _http.PostAsJsonAsync("api/CaseSheet/finalize", request);
                var rawResponse = await response.Content.ReadAsStringAsync();

                Console.WriteLine("--- FINALIZE CASE SHEET RESPONSE ---");
                Console.WriteLine(rawResponse);
                Console.WriteLine("-------------------------------------");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error finalizing case sheet: {ex.Message}");
                return false;
            }
        }

        public async Task<CaseSheetViewModel?> GetCaseSheetByVisitAsync(Guid? opId = null, Guid? ipId = null)
        {
            try
            {
                var queryParams = new List<string>();
                if (ipId.HasValue && ipId.Value != Guid.Empty)
                {
                    queryParams.Add($"ip_id={ipId.Value}");
                }
                else if (opId.HasValue && opId.Value != Guid.Empty)
                {
                    queryParams.Add($"op_id={opId.Value}");
                }

                var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
                var url = $"api/CaseSheet/by-visit{queryString}";

                Console.WriteLine($"--- GET CASE SHEET BY VISIT URL: {url} ---");
                var response = await _http.GetAsync(url);
                var rawJson = await response.Content.ReadAsStringAsync();

                Console.WriteLine("--- GET CASE SHEET BY VISIT RESPONSE ---");
                Console.WriteLine(rawJson);
                Console.WriteLine("----------------------------------------");

                if (response.IsSuccessStatusCode)
                {
                    var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var vm = System.Text.Json.JsonSerializer.Deserialize<CaseSheetViewModel>(rawJson, options);
                    AdjustViewModelDatesToIst(vm);
                    return vm;
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting case sheet by visit: {ex.Message}");
                return null;
            }
        }

        public async Task<CaseSheetPrescriptionViewModel?> GetPrescriptionByVisitAsync(Guid? opId = null, Guid? ipId = null)
        {
            try
            {
                var queryParams = new List<string>();
                if (ipId.HasValue && ipId.Value != Guid.Empty)
                {
                    queryParams.Add($"ip_id={ipId.Value}");
                }
                else if (opId.HasValue && opId.Value != Guid.Empty)
                {
                    queryParams.Add($"op_id={opId.Value}");
                }

                var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
                var response = await _http.GetFromJsonAsync<CaseSheetPrescriptionViewModel>($"api/CaseSheet/prescription{queryString}");
                AdjustPrescriptionDatesToIst(response);
                return response;
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
                        if (list != null)
                        {
                            foreach (var item in list)
                            {
                                AdjustViewModelDatesToIst(item);
                            }
                        }
                        return list ?? new List<CaseSheetViewModel>();
                    }
                    catch
                    {
                        // Fallback in case of wrapper
                        var wrapper = await response.Content.ReadFromJsonAsync<HistoryResponseWrapper>();
                        if (wrapper?.items != null)
                        {
                            foreach (var item in wrapper.items)
                            {
                                AdjustViewModelDatesToIst(item);
                            }
                        }
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

        public async Task<List<IcdItem>> GetAllIcd10Async()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<IcdItem>>("api/CaseSheet/getall");
                return response ?? new List<IcdItem>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting all ICD-10 codes: {ex.Message}");
                return new List<IcdItem>();
            }
        }

        public async Task<List<item_master>> GetAllItemsAsync()
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "api/ItemMaster/getallitems");
                request.Headers.Add("tenantcode", "TEN1011");
                
                var response = await _http.SendAsync(request);
                response.EnsureSuccessStatusCode();
                
                var rawJson = await response.Content.ReadAsStringAsync();
                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                
                if (rawJson.TrimStart().StartsWith("{"))
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(rawJson);
                    foreach (var prop in doc.RootElement.EnumerateObject())
                    {
                        if (prop.Value.ValueKind == System.Text.Json.JsonValueKind.Array)
                        {
                            return System.Text.Json.JsonSerializer.Deserialize<List<item_master>>(prop.Value.GetRawText(), options) ?? new List<item_master>();
                        }
                    }
                    Console.WriteLine("No array found in JSON object wrapper.");
                    return new List<item_master>();
                }
                
                var items = System.Text.Json.JsonSerializer.Deserialize<List<item_master>>(rawJson, options);
                return items ?? new List<item_master>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting all items: {ex.Message}");
                return new List<item_master>();
            }
        }

        public async Task<bool> UpdateInvestigationResultAsync(UpdateInvestigationResultRequest request)
        {
            try
            {
                var jsonOptions = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                var jsonPayload = System.Text.Json.JsonSerializer.Serialize(request, jsonOptions);
                Console.WriteLine("--- UPDATE INVESTIGATION RESULT PAYLOAD ---");
                Console.WriteLine(jsonPayload);
                Console.WriteLine("-------------------------------------------");

                var response = await _http.PostAsJsonAsync("api/CaseSheet/investigation/result", request);
                var rawResponse = await response.Content.ReadAsStringAsync();

                Console.WriteLine("--- UPDATE INVESTIGATION RESULT RESPONSE ---");
                Console.WriteLine(rawResponse);
                Console.WriteLine("--------------------------------------------");

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

        private static TimeZoneInfo GetIstTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            }
            catch
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
            }
        }

        private static DateTime? ConvertToIst(DateTime? utcDateTime)
        {
            if (!utcDateTime.HasValue) return null;
            var utc = DateTime.SpecifyKind(utcDateTime.Value, DateTimeKind.Utc);
            return TimeZoneInfo.ConvertTime(utc, GetIstTimeZone());
        }

        private static DateTime ConvertToIst(DateTime utcDateTime)
        {
            var utc = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
            return TimeZoneInfo.ConvertTime(utc, GetIstTimeZone());
        }

        private void AdjustViewModelDatesToIst(CaseSheetViewModel? vm)
        {
            if (vm == null) return;
            
            if (vm.visit_date.HasValue)
                vm.visit_date = ConvertToIst(vm.visit_date.Value);
            
            if (vm.followup_date.HasValue)
                vm.followup_date = ConvertToIst(vm.followup_date.Value);
            
            if (vm.diagnosis_list != null)
            {
                foreach (var diag in vm.diagnosis_list)
                {
                    diag.visit_date = ConvertToIst(diag.visit_date);
                }
            }

            if (vm.prescription != null)
            {
                if (vm.prescription.pr_date.HasValue)
                    vm.prescription.pr_date = ConvertToIst(vm.prescription.pr_date.Value);
            }

            if (vm.investigation != null)
            {
                if (vm.investigation.inv_date.HasValue)
                    vm.investigation.inv_date = ConvertToIst(vm.investigation.inv_date.Value);

                if (vm.investigation.tests != null)
                {
                    foreach (var test in vm.investigation.tests)
                    {
                        if (test.result_date.HasValue)
                            test.result_date = ConvertToIst(test.result_date.Value);
                    }
                }
            }
        }

        private void AdjustPrescriptionDatesToIst(CaseSheetPrescriptionViewModel? vm)
        {
            if (vm == null) return;
            if (vm.pr_date.HasValue)
                vm.pr_date = ConvertToIst(vm.pr_date.Value);
        }

        // Helper class to deserialize paginated histories if wrapped
        private class HistoryResponseWrapper
        {
            public List<CaseSheetViewModel> items { get; set; } = new();
            public int totalCount { get; set; }
        }
    }
}
