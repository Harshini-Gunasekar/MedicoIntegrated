using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using medico_backend.Model;

namespace Booking.Services
{
    public class HmsDueCollectionService
    {
        private readonly HttpClient _http;

        public HmsDueCollectionService(HttpClient http)
        {
            _http = http;
        }

        public async Task<HmsDueBillsResponse?> GetDueBillsResponseAsync(HmsAllDueFilterRequest filter)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/HmsDueCollection/due-bills", filter);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<HmsDueBillsResponse>();
                }
                else
                {
                    var query = BuildQueryString(filter);
                    var getResponse = await _http.GetAsync($"api/HmsDueCollection/due-bills?{query}");
                    if (getResponse.IsSuccessStatusCode)
                    {
                        return await getResponse.Content.ReadFromJsonAsync<HmsDueBillsResponse>();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetDueBillsResponseAsync: {ex.Message}");
            }
            return null;
        }

        public async Task<List<HmsAllDueBillRow>> GetDueBillsAsync(HmsAllDueFilterRequest filter)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/HmsDueCollection/due-bills", filter);
                if (response.IsSuccessStatusCode)
                {
                    var resObj = await response.Content.ReadFromJsonAsync<HmsDueBillsResponse>();
                    return resObj?.data ?? new();
                }
                else
                {
                    var query = BuildQueryString(filter);
                    var getResponse = await _http.GetAsync($"api/HmsDueCollection/due-bills?{query}");
                    if (getResponse.IsSuccessStatusCode)
                    {
                        var resObj = await getResponse.Content.ReadFromJsonAsync<HmsDueBillsResponse>();
                        return resObj?.data ?? new();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetDueBillsAsync: {ex.Message}");
                // Fallback attempt
                try
                {
                    var query = BuildQueryString(filter);
                    var resObj = await _http.GetFromJsonAsync<HmsDueBillsResponse>($"api/HmsDueCollection/due-bills?{query}");
                    return resObj?.data ?? new();
                }
                catch (Exception exFallback)
                {
                    Console.WriteLine($"Fallback error in GetDueBillsAsync: {exFallback.Message}");
                }
            }
            return new List<HmsAllDueBillRow>();
        }

        public async Task<HmsDuePreviewResponse?> PreviewAdvanceAsync(string requestGuid, double advanceToUse)
        {
            try
            {
                return await _http.GetFromJsonAsync<HmsDuePreviewResponse>($"api/HmsDueCollection/preview/{requestGuid}?advanceToUse={advanceToUse}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PreviewAdvanceAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<HmsDueCollectionResponse?> CollectDueAsync(HmsDueCollectionRequest request)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/HmsDueCollection/collect", request);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<HmsDueCollectionResponse>();
                }
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"CollectDueAsync failed status: {response.StatusCode}, error: {errorContent}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CollectDueAsync: {ex.Message}");
            }
            return null;
        }

        private string GetHistoryEndpoint(HmsDueCollectionFilterRequest filter)
        {
            return "api/HmsDueCollection/history/filter";
        }

        public async Task<List<HmsDueCollectionSummary>> GetDueHistoryAsync(HmsDueCollectionFilterRequest filter)
        {
            try
            {
                string endpoint = GetHistoryEndpoint(filter);
                var response = await _http.PostAsJsonAsync(endpoint, filter);
                if (response.IsSuccessStatusCode)
                {
                    var resObj = await response.Content.ReadFromJsonAsync<HmsDueHistoryResponse>();
                    return resObj?.data ?? new();
                }
                else
                {
                    var query = BuildQueryString(filter);
                    var getResponse = await _http.GetAsync($"{endpoint}?{query}");
                    if (getResponse.IsSuccessStatusCode)
                    {
                        var resObj = await getResponse.Content.ReadFromJsonAsync<HmsDueHistoryResponse>();
                        return resObj?.data ?? new();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetDueHistoryAsync: {ex.Message}");
                try
                {
                    string endpoint = GetHistoryEndpoint(filter);
                    var query = BuildQueryString(filter);
                    var resObj = await _http.GetFromJsonAsync<HmsDueHistoryResponse>($"{endpoint}?{query}");
                    return resObj?.data ?? new();
                }
                catch (Exception exFallback)
                {
                    Console.WriteLine($"Fallback error in GetDueHistoryAsync: {exFallback.Message}");
                }
            }
            return new List<HmsDueCollectionSummary>();
        }

        public async Task<HmsDueHistoryResponse?> GetDueHistoryResponseAsync(HmsDueCollectionFilterRequest filter)
        {
            try
            {
                string endpoint = GetHistoryEndpoint(filter);
                var response = await _http.PostAsJsonAsync(endpoint, filter);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<HmsDueHistoryResponse>();
                }
                else
                {
                    var query = BuildQueryString(filter);
                    var getResponse = await _http.GetAsync($"{endpoint}?{query}");
                    if (getResponse.IsSuccessStatusCode)
                    {
                        return await getResponse.Content.ReadFromJsonAsync<HmsDueHistoryResponse>();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetDueHistoryResponseAsync: {ex.Message}");
            }
            return null;
        }

        public async Task<HmsDueHistoryResponse?> GetUnfilteredDueHistoryResponseAsync(HmsDueCollectionFilterRequest filter)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/HmsDueCollection/list", filter);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<HmsDueHistoryResponse>();
                }
                else
                {
                    var query = BuildQueryString(filter);
                    var getResponse = await _http.GetAsync($"api/HmsDueCollection/list?{query}");
                    if (getResponse.IsSuccessStatusCode)
                    {
                        return await getResponse.Content.ReadFromJsonAsync<HmsDueHistoryResponse>();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetUnfilteredDueHistoryResponseAsync: {ex.Message}");
            }
            return null;
        }

        public async Task<string> CancelReceiptAsync(string receiptGuid, int usercode, string reason)
        {
            try
            {
                var response = await _http.GetAsync($"api/HmsDueCollection/cancel/{receiptGuid}?usercode={usercode}&reason={Uri.EscapeDataString(reason)}");
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CancelReceiptAsync: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }

        public async Task<HmsAdvanceReceiptResponse?> DepositAdvanceAsync(HmsAdvanceDepositRequest request)
        {
            try
            {
                var payloadStr = System.Text.Json.JsonSerializer.Serialize(request, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine($"\n>>> [API POST api/HmsDueCollection/advance/deposit]\nPayload:\n{payloadStr}\n");

                var response = await _http.PostAsJsonAsync("api/HmsDueCollection/advance/deposit", request);
                if (response.IsSuccessStatusCode)
                {
                    var wrapper = await response.Content.ReadFromJsonAsync<HmsAdvanceReceiptResponseWrapper>();
                    return wrapper?.data;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DepositAdvanceAsync: {ex.Message}");
            }
            return null;
        }

        public async Task<HmsPatientAdvanceSummary?> GetPatientAdvanceSummaryAsync(decimal custId)
        {
            try
            {
                var wrapper = await _http.GetFromJsonAsync<HmsPatientAdvanceSummaryWrapper>($"api/HmsDueCollection/advance-summary/{custId}");
                return wrapper?.data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetPatientAdvanceSummaryAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<HmsAdvanceReceiptResponse?> RefundAdvanceAsync(HmsAdvanceRefundRequest request)
        {
            try
            {
                var payloadStr = System.Text.Json.JsonSerializer.Serialize(request, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine($"\n>>> [API POST api/HmsDueCollection/advance/refund]\nPayload:\n{payloadStr}\n");

                var response = await _http.PostAsJsonAsync("api/HmsDueCollection/advance/refund", request);
                if (response.IsSuccessStatusCode)
                {
                    var wrapper = await response.Content.ReadFromJsonAsync<HmsAdvanceReceiptResponseWrapper>();
                    return wrapper?.data;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in RefundAdvanceAsync: {ex.Message}");
            }
            return null;
        }

        public async Task<HmsAdvanceReceiptResponse?> UseAdvanceAsync(HmsAdvanceUseRequest request)
        {
            try
            {
                var payloadStr = System.Text.Json.JsonSerializer.Serialize(request, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine($"\n>>> [API POST api/HmsDueCollection/advance/use]\nPayload:\n{payloadStr}\n");

                var response = await _http.PostAsJsonAsync("api/HmsDueCollection/advance/use", request);
                if (response.IsSuccessStatusCode)
                {
                    var wrapper = await response.Content.ReadFromJsonAsync<HmsAdvanceReceiptResponseWrapper>();
                    return wrapper?.data;
                }
                else
                {
                    var err = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error in UseAdvanceAsync status {response.StatusCode}: {err}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UseAdvanceAsync: {ex.Message}");
            }
            return null;
        }

        public async Task<HmsPaidHistoryResponse?> GetPaidHistoryAsync(HmsPaidHistoryFilterRequest filter)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/HmsDueCollection/paid-history", filter);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<HmsPaidHistoryResponse>();
                }
                else
                {
                    var query = BuildQueryString(filter);
                    var getResponse = await _http.GetAsync($"api/HmsDueCollection/paid-history?{query}");
                    if (getResponse.IsSuccessStatusCode)
                    {
                        return await getResponse.Content.ReadFromJsonAsync<HmsPaidHistoryResponse>();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetPaidHistoryAsync: {ex.Message}");
                try
                {
                    var query = BuildQueryString(filter);
                    return await _http.GetFromJsonAsync<HmsPaidHistoryResponse>($"api/HmsDueCollection/paid-history?{query}");
                }
                catch (Exception exFallback)
                {
                    Console.WriteLine($"Fallback error in GetPaidHistoryAsync: {exFallback.Message}");
                }
            }
            return null;
        }

        /// <summary>
        /// Bulk due collection — one shared receipt for all selected bills.
        /// POST api/HmsDueCollection/collect/bulk
        /// </summary>
        public async Task<HmsBulkDueCollectionResponse?> CollectBulkDueAsync(HmsBulkDueCollectionRequest request)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/HmsDueCollection/collect/bulk", request);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<HmsBulkDueCollectionResponse>();
                }
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"CollectBulkDueAsync failed status: {response.StatusCode}, error: {errorContent}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CollectBulkDueAsync: {ex.Message}");
            }
            return null;
        }

        /// <summary>
        /// Filter due bills shown in the Collect Dues (Save) tab.
        /// POST api/HmsDueCollection/save/filter
        /// </summary>
        public async Task<HmsDailyCollectionReportResponse?> GetSaveFilterAsync(HmsDailyCollectionReportFilterRequest filter)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/HmsDueCollection/save/filter", filter);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<HmsDailyCollectionReportResponse>();
                }
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"GetSaveFilterAsync failed status: {response.StatusCode}, error: {errorContent}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetSaveFilterAsync: {ex.Message}");
            }
            return null;
        }

        /// <summary>
        /// Filter collection history rows.
        /// POST api/HmsDueCollection/history/filter
        /// </summary>
        public async Task<HmsDailyCollectionReportResponse?> GetHistoryFilterAsync(HmsDailyCollectionReportFilterRequest filter)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/HmsDueCollection/history/filter", filter);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<HmsDailyCollectionReportResponse>();
                }
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"GetHistoryFilterAsync failed status: {response.StatusCode}, error: {errorContent}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetHistoryFilterAsync: {ex.Message}");
            }
            return null;
        }

        private string BuildQueryString(object obj)
        {
            var properties = from p in obj.GetType().GetProperties()
                             let value = p.GetValue(obj, null)
                             where value != null
                             select $"{p.Name}={Uri.EscapeDataString(FormatValue(value))}";
            return string.Join("&", properties);
        }

        private string FormatValue(object val)
        {
            if (val is DateTime dt)
            {
                return dt.ToString("yyyy-MM-dd");
            }
            return val.ToString() ?? "";
        }

        #region Advance & Refund Receipt Print APIs

        /// <summary>
        /// Builds Advance Receipt Print API URL:
        /// GET /api/report/getadvancereceipt?receiptguid={receiptguid}&pageformat=A4&tenant_code={tenant_code}
        /// Parameters:
        /// - receiptguid: Receipt GUID or Receipt No (single receipt print)
        /// - custid: Patient ID (prints all receipts for patient, each on separate page)
        /// - admissionno: IP Admission No
        /// - pageformat: "A4" or "A5" (optional, default: "A4")
        /// - format: "pdf" for raw file download, or omit to get Base64 string (default)
        /// - tenant_code: Hospital tenant code (optional, uses session if omitted)
        /// </summary>
        public string BuildAdvanceReceiptUrl(
            string? receiptguid = null,
            decimal? custid = null,
            string? admissionno = null,
            string pageformat = "A4",
            string? format = null,
            string? tenant_code = null)
        {
            var qb = new List<string>();
            if (!string.IsNullOrWhiteSpace(receiptguid))
                qb.Add($"receiptguid={Uri.EscapeDataString(receiptguid)}");
            if (custid.HasValue && custid.Value > 0)
                qb.Add($"custid={custid.Value}");
            if (!string.IsNullOrWhiteSpace(admissionno))
                qb.Add($"admissionno={Uri.EscapeDataString(admissionno)}");
            if (!string.IsNullOrWhiteSpace(pageformat))
                qb.Add($"pageformat={Uri.EscapeDataString(pageformat)}");
            if (!string.IsNullOrWhiteSpace(format))
                qb.Add($"format={Uri.EscapeDataString(format)}");
            if (!string.IsNullOrWhiteSpace(tenant_code))
                qb.Add($"tenant_code={Uri.EscapeDataString(tenant_code)}");

            var query = qb.Count > 0 ? "?" + string.Join("&", qb) : "";
            return $"api/report/getadvancereceipt{query}";
        }

        public string BuildAdvanceReceiptUrl(ReceiptPrintOptions options)
        {
            if (options == null) return "api/report/getadvancereceipt?pageformat=A4";
            return BuildAdvanceReceiptUrl(options.receiptguid, options.custid, options.admissionno, options.pageformat, options.format, options.tenant_code);
        }

        /// <summary>
        /// Builds Refund Receipt Print API URL:
        /// GET /api/report/getrefundreceipt?receiptguid={receiptguid}&pageformat=A4&tenant_code={tenant_code}
        /// Parameters:
        /// - receiptguid: Receipt GUID or Receipt No (single receipt print)
        /// - custid: Patient ID (prints all receipts for patient, each on separate page)
        /// - admissionno: IP Admission No
        /// - pageformat: "A4" or "A5" (optional, default: "A4")
        /// - format: "pdf" for raw file download, or omit to get Base64 string (default)
        /// - tenant_code: Hospital tenant code (optional, uses session if omitted)
        /// </summary>
        public string BuildRefundReceiptUrl(
            string? receiptguid = null,
            decimal? custid = null,
            string? admissionno = null,
            string pageformat = "A4",
            string? format = null,
            string? tenant_code = null)
        {
            var qb = new List<string>();
            if (!string.IsNullOrWhiteSpace(receiptguid))
                qb.Add($"receiptguid={Uri.EscapeDataString(receiptguid)}");
            if (custid.HasValue && custid.Value > 0)
                qb.Add($"custid={custid.Value}");
            if (!string.IsNullOrWhiteSpace(admissionno))
                qb.Add($"admissionno={Uri.EscapeDataString(admissionno)}");
            if (!string.IsNullOrWhiteSpace(pageformat))
                qb.Add($"pageformat={Uri.EscapeDataString(pageformat)}");
            if (!string.IsNullOrWhiteSpace(format))
                qb.Add($"format={Uri.EscapeDataString(format)}");
            if (!string.IsNullOrWhiteSpace(tenant_code))
                qb.Add($"tenant_code={Uri.EscapeDataString(tenant_code)}");

            var query = qb.Count > 0 ? "?" + string.Join("&", qb) : "";
            return $"api/report/getrefundreceipt{query}";
        }

        public string BuildRefundReceiptUrl(ReceiptPrintOptions options)
        {
            if (options == null) return "api/report/getrefundreceipt?pageformat=A4";
            return BuildRefundReceiptUrl(options.receiptguid, options.custid, options.admissionno, options.pageformat, options.format, options.tenant_code);
        }

        /// <summary>
        /// Calls GET /api/report/getadvancereceipt and returns Base64-encoded PDF string.
        /// </summary>
        public async Task<string?> GetAdvanceReceiptBase64Async(
            string? receiptguid = null,
            decimal? custid = null,
            string? admissionno = null,
            string pageformat = "A4",
            string? tenant_code = null)
        {
            try
            {
                var url = BuildAdvanceReceiptUrl(receiptguid, custid, admissionno, pageformat, format: null, tenant_code: tenant_code);
                Console.WriteLine($"[GetAdvanceReceiptBase64Async] Calling GET {url}");
                var response = await _http.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var bytes = await response.Content.ReadAsByteArrayAsync();
                    if (bytes == null || bytes.Length == 0) return null;

                    // If binary PDF (%PDF-) returned
                    if (bytes.Length >= 5 && bytes[0] == 0x25 && bytes[1] == 0x50 && bytes[2] == 0x44 && bytes[3] == 0x46 && bytes[4] == 0x2D)
                    {
                        return Convert.ToBase64String(bytes);
                    }
                    var str = System.Text.Encoding.UTF8.GetString(bytes).Trim().Trim('"').Trim();
                    return str;
                }
                else
                {
                    var err = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[GetAdvanceReceiptBase64Async] Failed {response.StatusCode}: {err}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetAdvanceReceiptBase64Async] Exception: {ex.Message}");
            }
            return null;
        }

        public Task<string?> GetAdvanceReceiptBase64Async(ReceiptPrintOptions options)
        {
            if (options == null) return Task.FromResult<string?>(null);
            return GetAdvanceReceiptBase64Async(options.receiptguid, options.custid, options.admissionno, options.pageformat, options.tenant_code);
        }

        /// <summary>
        /// Calls GET /api/report/getrefundreceipt and returns Base64-encoded PDF string.
        /// </summary>
        public async Task<string?> GetRefundReceiptBase64Async(
            string? receiptguid = null,
            decimal? custid = null,
            string? admissionno = null,
            string pageformat = "A4",
            string? tenant_code = null)
        {
            try
            {
                var url = BuildRefundReceiptUrl(receiptguid, custid, admissionno, pageformat, format: null, tenant_code: tenant_code);
                Console.WriteLine($"[GetRefundReceiptBase64Async] Calling GET {url}");
                var response = await _http.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var bytes = await response.Content.ReadAsByteArrayAsync();
                    if (bytes == null || bytes.Length == 0) return null;

                    // If binary PDF (%PDF-) returned
                    if (bytes.Length >= 5 && bytes[0] == 0x25 && bytes[1] == 0x50 && bytes[2] == 0x44 && bytes[3] == 0x46 && bytes[4] == 0x2D)
                    {
                        return Convert.ToBase64String(bytes);
                    }
                    var str = System.Text.Encoding.UTF8.GetString(bytes).Trim().Trim('"').Trim();
                    return str;
                }
                else
                {
                    var err = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[GetRefundReceiptBase64Async] Failed {response.StatusCode}: {err}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetRefundReceiptBase64Async] Exception: {ex.Message}");
            }
            return null;
        }

        public Task<string?> GetRefundReceiptBase64Async(ReceiptPrintOptions options)
        {
            if (options == null) return Task.FromResult<string?>(null);
            return GetRefundReceiptBase64Async(options.receiptguid, options.custid, options.admissionno, options.pageformat, options.tenant_code);
        }

        /// <summary>
        /// Calls GET /api/report/getadvancereceipt with format=pdf and returns raw PDF binary bytes.
        /// </summary>
        public async Task<byte[]?> GetAdvanceReceiptPdfBytesAsync(
            string? receiptguid = null,
            decimal? custid = null,
            string? admissionno = null,
            string pageformat = "A4",
            string? tenant_code = null)
        {
            try
            {
                var url = BuildAdvanceReceiptUrl(receiptguid, custid, admissionno, pageformat, format: "pdf", tenant_code: tenant_code);
                var response = await _http.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var bytes = await response.Content.ReadAsByteArrayAsync();
                    if (bytes == null || bytes.Length == 0) return null;

                    if (bytes.Length >= 5 && bytes[0] == 0x25 && bytes[1] == 0x50 && bytes[2] == 0x44 && bytes[3] == 0x46 && bytes[4] == 0x2D)
                    {
                        return bytes;
                    }
                    var str = System.Text.Encoding.UTF8.GetString(bytes).Trim().Trim('"').Trim();
                    return Convert.FromBase64String(str);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetAdvanceReceiptPdfBytesAsync] Exception: {ex.Message}");
            }
            return null;
        }

        /// <summary>
        /// Calls GET /api/report/getrefundreceipt with format=pdf and returns raw PDF binary bytes.
        /// </summary>
        public async Task<byte[]?> GetRefundReceiptPdfBytesAsync(
            string? receiptguid = null,
            decimal? custid = null,
            string? admissionno = null,
            string pageformat = "A4",
            string? tenant_code = null)
        {
            try
            {
                var url = BuildRefundReceiptUrl(receiptguid, custid, admissionno, pageformat, format: "pdf", tenant_code: tenant_code);
                var response = await _http.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var bytes = await response.Content.ReadAsByteArrayAsync();
                    if (bytes == null || bytes.Length == 0) return null;

                    if (bytes.Length >= 5 && bytes[0] == 0x25 && bytes[1] == 0x50 && bytes[2] == 0x44 && bytes[3] == 0x46 && bytes[4] == 0x2D)
                    {
                        return bytes;
                    }
                    var str = System.Text.Encoding.UTF8.GetString(bytes).Trim().Trim('"').Trim();
                    return Convert.FromBase64String(str);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetRefundReceiptPdfBytesAsync] Exception: {ex.Message}");
            }
            return null;
        }

        #endregion
    }
}

namespace medico_backend.Model
{
    public class HmsDueBillsResponse
    {
        public bool success { get; set; }
        public int totalCount { get; set; }
        public int page { get; set; }
        public int pagesize { get; set; }
        public HmsAllDueSummary? summary { get; set; }
        public List<HmsAllDueBillRow> data { get; set; } = new();
    }

    public class HmsDueHistoryResponse
    {
        public bool success { get; set; }
        public int totalCount { get; set; }
        public int page { get; set; }
        public int pagesize { get; set; }
        public List<HmsDueCollectionSummary> data { get; set; } = new();
    }

    public class HmsPaidHistoryResponse
    {
        public bool success { get; set; }
        public int totalCount { get; set; }
        public int page { get; set; }
        public int pagesize { get; set; }
        public HmsPaidHistorySummary? summary { get; set; }
        public List<HmsPaidHistoryRow> data { get; set; } = new();
    }

    public class HmsDailyCollectionReportResponse
    {
        public bool success { get; set; }
        public int totalCount { get; set; }
        public int page { get; set; }
        public int pagesize { get; set; }
        public HmsDailyCollectionReportSummary? summary { get; set; }
        public List<HmsDailyCollectionReportRow> data { get; set; } = new();
    }

    public class HmsPatientAdvanceSummaryWrapper
    {
        public bool success { get; set; }
        public string? message { get; set; }
        public HmsPatientAdvanceSummary? data { get; set; }
    }

    public class HmsAdvanceReceiptResponseWrapper
    {
        public bool success { get; set; }
        public string? message { get; set; }
        public HmsAdvanceReceiptResponse? data { get; set; }
    }
}
