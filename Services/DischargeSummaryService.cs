using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Booking.Services
{
    public class DischargeSummaryService
    {
        private readonly HttpClient _http;

        public DischargeSummaryService(HttpClient http)
        {
            _http = http;
        }

        // Models
        public class DsCategoryDto
        {
            public Guid category_id { get; set; }
            public string category_name { get; set; } = string.Empty;
            public string category_type { get; set; } = "TEXT"; // "TEXT", "MEDICINE", "BOTH"
            public int sort_order { get; set; } = 0;
            public bool is_active { get; set; } = true;
        }

        public class DsTemplateDto
        {
            public Guid template_id { get; set; }
            public string template_name { get; set; } = string.Empty;
            public Guid category_id { get; set; }
            public string category_name { get; set; } = string.Empty;
            public string template_text { get; set; } = string.Empty;
        }

        public class PdsMasterDto
        {
            public Guid pds_id { get; set; }
            public string patcode { get; set; } = string.Empty;
            public long custid { get; set; }
            public Guid? op_id { get; set; }
            public int? dcode { get; set; }
            public string? patient_name { get; set; }
            public string? gender { get; set; }
            public string? age { get; set; }
            public string? mobile_no { get; set; }
            public string? doctor_name { get; set; }
            public string? bed_no { get; set; }
            public DateTime? admission_date { get; set; }
            public DateTime? discharge_date { get; set; }
            public string? discharge_type { get; set; } = "NORMAL";
            public string? overall_notes { get; set; }
            public string? auth_user1 { get; set; }
            public string? auth_user2 { get; set; }
            public string? auth_user3 { get; set; }
        }

        public class PdsDetailDto
        {
            public Guid pds_detail_id { get; set; }
            public Guid pds_id { get; set; }
            public Guid category_id { get; set; }
            public string category_name { get; set; } = string.Empty;
            public string category_content { get; set; } = string.Empty;
            public int sort_order { get; set; } = 0;
        }

        public class SavePatientDischargeSummaryDto
        {
            public Guid? pds_id { get; set; }
            public string patcode { get; set; } = string.Empty;
            public long custid { get; set; }
            public Guid? op_id { get; set; }
            public int? dcode { get; set; }
            public string? patient_name { get; set; }
            public string? gender { get; set; }
            public string? age { get; set; }
            public string? mobile_no { get; set; }
            public string? doctor_name { get; set; }
            public string? bed_no { get; set; }
            public DateTime? admission_date { get; set; }
            public DateTime? discharge_date { get; set; }
            public string? discharge_type { get; set; } = "NORMAL";
            public string? overall_notes { get; set; }
            public string? auth_user1 { get; set; }
            public string? auth_user2 { get; set; }
            public string? auth_user3 { get; set; }
            public List<PdsCategoryContentDto> details { get; set; } = new();
        }

        public class PdsCategoryContentDto
        {
            public Guid category_id { get; set; }
            public string category_name { get; set; } = string.Empty;
            public string category_content { get; set; } = string.Empty;
            public int sort_order { get; set; } = 0;
        }

        public class PatientDischargeSummaryResponse
        {
            public PdsMasterDto Master { get; set; } = new();
            public List<PdsDetailDto> Details { get; set; } = new();
            public string PatientName { get; set; } = string.Empty;
            public string PatientId { get; set; } = string.Empty;
            public string Gender { get; set; } = string.Empty;
            public string Age { get; set; } = string.Empty;
            public string MobileNo { get; set; } = string.Empty;
            public string DoctorName { get; set; } = string.Empty;
            public string BedNo { get; set; } = string.Empty;
        }

        public class ApiResponseWrapper<T>
        {
            public bool success { get; set; }
            public string? message { get; set; }
            public T? data { get; set; }
        }

        // Methods
        public async Task<List<DsCategoryDto>> GetCategoriesAsync()
        {
            try
            {
                var res = await _http.GetFromJsonAsync<ApiResponseWrapper<List<DsCategoryDto>>>("api/DischargeSummary/categories");
                return res?.data ?? new List<DsCategoryDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching discharge summary categories: {ex.Message}");
                return new List<DsCategoryDto>();
            }
        }

        public async Task<Guid?> SaveCategoryAsync(DsCategoryDto dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/DischargeSummary/category", dto);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    dynamic? obj = JsonConvert.DeserializeObject(json);
                    if (obj != null && obj.category_id != null)
                    {
                        return (Guid)obj.category_id;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving discharge summary category: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> DeleteCategoryAsync(Guid categoryId)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/DischargeSummary/category/{categoryId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting discharge summary category: {ex.Message}");
                return false;
            }
        }

        public async Task<List<DsTemplateDto>> GetTemplatesAsync(Guid? categoryId = null)
        {
            try
            {
                string url = "api/DischargeSummary/templates";
                if (categoryId.HasValue && categoryId.Value != Guid.Empty)
                {
                    url += $"?category_id={categoryId.Value}";
                }
                var res = await _http.GetFromJsonAsync<ApiResponseWrapper<List<DsTemplateDto>>>(url);
                return res?.data ?? new List<DsTemplateDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching discharge summary templates: {ex.Message}");
                return new List<DsTemplateDto>();
            }
        }

        public async Task<Guid?> SaveTemplateAsync(DsTemplateDto dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/DischargeSummary/template", dto);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    dynamic? obj = JsonConvert.DeserializeObject(json);
                    if (obj != null && obj.template_id != null)
                    {
                        return (Guid)obj.template_id;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving discharge summary template: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> DeleteTemplateAsync(Guid templateId)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/DischargeSummary/template/{templateId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting discharge summary template: {ex.Message}");
                return false;
            }
        }

        public async Task<PatientDischargeSummaryResponse?> GetPatientDischargeSummaryAsync(string patcodeOrPdsId)
        {
            try
            {
                var encoded = Uri.EscapeDataString(patcodeOrPdsId ?? "");
                var res = await _http.GetFromJsonAsync<ApiResponseWrapper<PatientDischargeSummaryResponse>>($"api/DischargeSummary/patient?code={encoded}");
                return res?.data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching patient discharge summary: {ex.Message}");
                return null;
            }
        }

        public async Task<Guid?> SavePatientDischargeSummaryAsync(SavePatientDischargeSummaryDto dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/DischargeSummary/patient/save", dto);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    dynamic? obj = JsonConvert.DeserializeObject(json);
                    if (obj != null && obj.pds_id != null)
                    {
                        return (Guid)obj.pds_id;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving patient discharge summary: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> AuthorizePatientDischargeSummaryAsync(Guid pdsId, string? authUser1, string? authUser2, string? authUser3)
        {
            try
            {
                var payload = new { pds_id = pdsId, auth_user1 = authUser1, auth_user2 = authUser2, auth_user3 = authUser3 };
                var response = await _http.PostAsJsonAsync("api/DischargeSummary/patient/authorize", payload);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error authorizing discharge summary: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Calls the backend api/Report/getdischargesummary?pds_id={guid} endpoint
        /// which proxies to the ReportingServer and returns a base64 PDF string.
        /// This is the same pattern as api/Report/getbill and api/Report/getopcasesheet.
        /// </summary>
        public async Task<string?> GetDischargeSummaryPdfBase64Async(Guid pds_id)
        {
            try
            {
                var response = await _http.GetAsync($"api/Report/getdischargesummary?pds_id={pds_id}&isletterhead=true");
                if (response.IsSuccessStatusCode)
                {
                    var raw = await response.Content.ReadAsStringAsync();
                    return raw.Trim().Trim('"');
                }
                var err = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[DischargeSummaryService] PDF generation failed ({response.StatusCode}): {err}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DischargeSummaryService] GetDischargeSummaryPdfBase64Async error: {ex.Message}");
                return null;
            }
        }
    }
}
