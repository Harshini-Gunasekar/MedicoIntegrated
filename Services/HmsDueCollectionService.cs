using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using medico_backend.Model;

namespace Booking.Services
{
    public class HmsDueCollectionService
    {
        private readonly HttpClient _http;

        private readonly IConfiguration? _config;

        public HmsDueCollectionService(HttpClient http, IConfiguration? config = null)
        {
            _http = http;
            _config = config;
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

        public async Task<List<HmsAdvancePatientItem>> GetAllAdvancePatientsAsync(string? tenantCode = null)
        {
            try
            {
                var response = await _http.GetAsync("api/HmsDueCollection/advance/all-patients");
                if (response.IsSuccessStatusCode)
                {
                    var res = await response.Content.ReadFromJsonAsync<HmsAdvancePatientsResponseWrapper>();
                    if (res?.data != null && res.data.Count > 0)
                    {
                        return res.data;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetAllAdvancePatientsAsync] API attempt: {ex.Message}");
            }

            return new List<HmsAdvancePatientItem>();
        }

        public async Task<(bool success, string message, HmsAdvanceCancelResponse? data)> CancelAdvanceAsync(HmsAdvanceCancelRequest request)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/HmsDueCollection/advance/cancel", request);
                if (response.IsSuccessStatusCode)
                {
                    var res = await response.Content.ReadFromJsonAsync<HmsAdvanceCancelResponseWrapper>();
                    if (res?.data != null)
                    {
                        return (true, res.message ?? "Advance receipt cancelled successfully.", res.data);
                    }
                    return (true, res?.message ?? "Advance receipt cancelled successfully.", null);
                }
                else
                {
                    var err = await response.Content.ReadFromJsonAsync<HmsAdvanceCancelResponseWrapper>();
                    if (!string.IsNullOrWhiteSpace(err?.message))
                    {
                        return (false, err.message, null);
                    }
                    return (false, $"Cancellation failed with status {response.StatusCode}", null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CancelAdvanceAsync] API attempt: {ex.Message}");
                return (false, $"Cancellation error: {ex.Message}", null);
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

        #region Advance Requests Queue & Refund History

        private static readonly List<AdvanceRequestItem> _advanceRequests = new();

        public async Task<List<AdvanceRequestItem>> GetAdvanceRequestsAsync(string status = "PENDING", DateTime? fromDate = null, DateTime? toDate = null)
        {
            // 1. Try Backend API
            try
            {
                var reqObj = new
                {
                    status = string.IsNullOrWhiteSpace(status) || string.Equals(status, "ALL", StringComparison.OrdinalIgnoreCase) ? null : (status == "RECEIVED" ? "COLLECTED" : status),
                    fromdate = fromDate.HasValue ? fromDate.Value.ToString("yyyy-MM-dd") : null,
                    todate = toDate.HasValue ? toDate.Value.ToString("yyyy-MM-dd") : null,
                    page = 1,
                    pagesize = 100
                };
                var response = await _http.PostAsJsonAsync("api/HmsDueCollection/advance/requests", reqObj);
                if (response.IsSuccessStatusCode)
                {
                    var apiRes = await response.Content.ReadFromJsonAsync<HmsAdvanceRequestsBackendResponse>();
                    if (apiRes?.data != null)
                    {
                        return apiRes.data.Select(r => new AdvanceRequestItem
                        {
                            request_id = r.request_id,
                            request_no = r.request_no,
                            request_time = r.request_date,
                            custid = r.custid,
                            custcode = string.IsNullOrWhiteSpace(r.custcode) ? $"CUST-{r.custid}" : r.custcode,
                            patient_name = r.patient_name,
                            mobile = r.mobile_no,
                            amount = r.amount,
                            source_type = r.purpose ?? "Billing",
                            notes = r.remarks,
                            status = r.status == "COLLECTED" ? "RECEIVED" : r.status,
                            target_cntcode = r.target_cntcode,
                            receiptguid = r.receiptguid,
                            receiptsnoprint = r.receiptsnoprint
                        }).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetAdvanceRequestsAsync] API call: {ex.Message}");
            }

            // Fallback
            lock (_advanceRequests)
            {
                var query = _advanceRequests.AsEnumerable();
                if (!string.Equals(status, "ALL", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(status))
                    query = query.Where(r => string.Equals(r.status, status, StringComparison.OrdinalIgnoreCase) || (string.Equals(status, "RECEIVED", StringComparison.OrdinalIgnoreCase) && string.Equals(r.status, "COLLECTED", StringComparison.OrdinalIgnoreCase)));
                if (fromDate.HasValue)
                    query = query.Where(r => r.request_time.Date >= fromDate.Value.Date);
                if (toDate.HasValue)
                    query = query.Where(r => r.request_time.Date <= toDate.Value.Date);

                return query.OrderByDescending(r => r.request_time).ToList();
            }
        }

        public async Task<AdvanceRequestsSummary> GetAdvanceRequestsSummaryAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var summary = new AdvanceRequestsSummary();

            // 1. Try Backend API
            try
            {
                var reqObj = new
                {
                    fromdate = fromDate.HasValue ? fromDate.Value.ToString("yyyy-MM-dd") : null,
                    todate = toDate.HasValue ? toDate.Value.ToString("yyyy-MM-dd") : null,
                    page = 1,
                    pagesize = 1
                };
                var response = await _http.PostAsJsonAsync("api/HmsDueCollection/advance/requests", reqObj);
                if (response.IsSuccessStatusCode)
                {
                    var apiRes = await response.Content.ReadFromJsonAsync<HmsAdvanceRequestsBackendResponse>();
                    if (apiRes?.summary != null)
                    {
                        summary.pending_count = apiRes.summary.pending_count;
                        summary.pending_amount = apiRes.summary.pending_amount;
                        summary.received_count = apiRes.summary.collected_count;
                        summary.received_amount = apiRes.summary.collected_amount;
                        return summary;
                    }
                }
            }
            catch { }

            // 2. In-memory calculation
            var all = await GetAdvanceRequestsAsync("ALL");
            var today = DateTime.UtcNow.Date;
            summary.pending_count = all.Count(r => r.status == "PENDING");
            summary.pending_amount = all.Where(r => r.status == "PENDING").Sum(r => r.amount);
            summary.received_count = all.Count(r => (r.status == "RECEIVED" || r.status == "COLLECTED") && r.request_time.Date == today);
            summary.received_amount = all.Where(r => (r.status == "RECEIVED" || r.status == "COLLECTED") && r.request_time.Date == today).Sum(r => r.amount);
            return summary;
        }

        public async Task<AdvanceRequestItem> CreateAdvanceRequestAsync(AdvanceRequestItem request)
        {
            // 1. Try Backend API
            try
            {
                var reqDto = new
                {
                    custid = request.custid,
                    patient_name = request.patient_name,
                    mobile_no = request.mobile,
                    amount = request.amount,
                    purpose = request.source_type,
                    remarks = request.notes,
                    target_cntcode = request.target_cntcode,
                    usercode = request.requested_by,
                    user_name = request.requested_by_name
                };
                var response = await _http.PostAsJsonAsync("api/HmsDueCollection/advance/request", reqDto);
                if (response.IsSuccessStatusCode)
                {
                    var wrapper = await response.Content.ReadFromJsonAsync<HmsAdvanceRequestCreateBackendResponse>();
                    if (wrapper?.data != null)
                    {
                        request.request_id = wrapper.data.request_id;
                        request.request_no = wrapper.data.request_no;
                        request.request_time = wrapper.data.request_date;
                        request.status = wrapper.data.status;
                        return request;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CreateAdvanceRequestAsync] API error: {ex.Message}");
            }

            // Fallback
            lock (_advanceRequests)
            {
                if (string.IsNullOrWhiteSpace(request.request_no))
                {
                    request.request_no = $"AR-{DateTime.Now:yyyyMMdd}-{(_advanceRequests.Count + 101):D4}";
                }
                request.request_time = DateTime.UtcNow;
                request.status = "PENDING";
                _advanceRequests.Insert(0, request);
            }
            return request;
        }

        public async Task<bool> CancelAdvanceRequestAsync(string requestNo, string? reason = null, Guid? requestId = null)
        {
            // 1. Try Backend API
            try
            {
                var reqDto = new
                {
                    request_id = requestId ?? Guid.Empty,
                    cancel_reason = reason ?? "Cancelled by user"
                };
                var response = await _http.PostAsJsonAsync("api/HmsDueCollection/advance/request/cancel", reqDto);
                if (response.IsSuccessStatusCode) return true;
            }
            catch { }

            // Fallback
            lock (_advanceRequests)
            {
                var match = _advanceRequests.FirstOrDefault(r => r.request_no == requestNo || (requestId.HasValue && r.request_id == requestId.Value));
                if (match != null)
                {
                    match.status = "CANCELLED";
                    if (!string.IsNullOrWhiteSpace(reason)) match.notes = (match.notes ?? "") + $" [Cancelled: {reason}]";
                    return true;
                }
            }
            return false;
        }

        public async Task<(bool success, string? message, HmsAdvanceReceiptResponse? receipt)> ReceiveAdvanceRequestAsync(
            string requestNo, HmsAdvanceDepositRequest depositReq, Guid? requestId = null)
        {
            // 1. Try Backend API
            try
            {
                var collectDto = new
                {
                    request_id = requestId ?? Guid.Empty,
                    amount = depositReq.amount,
                    collection_type = string.IsNullOrWhiteSpace(depositReq.collection_type) ? "CASH" : depositReq.collection_type,
                    enteredbhcode = depositReq.enteredbhcode,
                    cntcode = depositReq.cntcode,
                    usercode = depositReq.usercode,
                    computercode = depositReq.computercode,
                    remarks = depositReq.remarks
                };
                var response = await _http.PostAsJsonAsync("api/HmsDueCollection/advance/request/receive", collectDto);
                if (response.IsSuccessStatusCode)
                {
                    var wrapper = await response.Content.ReadFromJsonAsync<HmsAdvanceReceiptResponseWrapper>();
                    if (wrapper?.data != null)
                    {
                        UpdateLocalRequestStatus(requestNo, "RECEIVED", wrapper.data.receipt_guid, wrapper.data.receipt_no);
                        return (true, "Advance collection received successfully.", wrapper.data);
                    }
                }
            }
            catch { }

            // 2. Fallback to standard advance deposit
            var depositRes = await DepositAdvanceAsync(depositReq);
            if (depositRes != null)
            {
                UpdateLocalRequestStatus(requestNo, "RECEIVED", depositRes.receipt_guid, depositRes.receipt_no);
                return (true, "Advance collection received successfully.", depositRes);
            }

            return (false, "Failed to record advance deposit receipt.", null);
        }

        private void UpdateLocalRequestStatus(string requestNo, string status, string? receiptGuid, string? receiptNo)
        {
            lock (_advanceRequests)
            {
                var match = _advanceRequests.FirstOrDefault(r => r.request_no == requestNo);
                if (match != null)
                {
                    match.status = status;
                    match.receiptguid = receiptGuid;
                    match.receiptsnoprint = receiptNo;
                }
            }
        }

        public async Task<(List<HmsAdvancePatientItem> data, PatientDepositsSummary summary)> GetPatientDepositsAsync(
            string balanceStatus = "HAS_BALANCE", string? search = null, string? tenantCode = null)
        {
            var summary = new PatientDepositsSummary();

            // 1. Try Backend API
            try
            {
                var reqObj = new
                {
                    balance_status = balanceStatus,
                    search = search,
                    page = 1,
                    pagesize = 100
                };
                var res = await _http.PostAsJsonAsync("api/HmsDueCollection/advance/patient-deposits", reqObj);
                if (res.IsSuccessStatusCode)
                {
                    var pRes = await res.Content.ReadFromJsonAsync<HmsPatientAdvanceDepositBackendResponse>();
                    if (pRes?.data != null && pRes.data.Count > 0)
                    {
                        var list = pRes.data.Select(x => new HmsAdvancePatientItem
                        {
                            custid = x.custid,
                            custcode = !string.IsNullOrWhiteSpace(x.custcode) ? x.custcode : x.custid.ToString(),
                            patient_name = x.patient_name,
                            mobile = x.mobile_no,
                            total_deposited = x.total_deposited,
                            total_used = x.total_used,
                            total_refunded = x.total_refunded,
                            available_balance = x.available_balance,
                            last_deposit_date = x.last_deposit_date
                        }).ToList();

                        if (pRes.summary != null)
                        {
                            summary.total_deposited_sum = pRes.summary.total_deposited;
                            summary.total_used_sum = pRes.summary.total_used;
                            summary.total_refunded_sum = pRes.summary.total_refunded;
                            summary.total_available_sum = pRes.summary.total_available_balance;
                        }
                        else
                        {
                            summary.total_deposited_sum = list.Sum(x => x.total_deposited);
                            summary.total_used_sum = list.Sum(x => x.total_used);
                            summary.total_refunded_sum = list.Sum(x => x.total_refunded);
                            summary.total_available_sum = list.Sum(x => x.available_balance);
                        }
                        return (list, summary);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetPatientDepositsAsync] API error: {ex.Message}");
            }

            // Fallback to GetAllAdvancePatientsAsync
            var allPatients = await GetAllAdvancePatientsAsync(tenantCode);
            var query = allPatients.AsEnumerable();

            if (string.Equals(balanceStatus, "HAS_BALANCE", StringComparison.OrdinalIgnoreCase))
                query = query.Where(p => p.available_balance > 0);
            else if (string.Equals(balanceStatus, "ZERO_BALANCE", StringComparison.OrdinalIgnoreCase))
                query = query.Where(p => p.available_balance <= 0);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    (p.patient_name != null && p.patient_name.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (p.mobile != null && p.mobile.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    p.custid.ToString().Contains(search)
                );
            }

            var filtered = query.ToList();
            summary.total_deposited_sum = filtered.Sum(x => x.total_deposited);
            summary.total_used_sum = filtered.Sum(x => x.total_used);
            summary.total_refunded_sum = filtered.Sum(x => x.total_refunded);
            summary.total_available_sum = filtered.Sum(x => x.available_balance);

            return (filtered, summary);
        }

        public async Task<List<AdvanceRefundHistoryItem>> GetRefundHistoryAsync(
            DateTime? fromDate = null, DateTime? toDate = null, string? search = null, string? tenantCode = null)
        {
            // 1. Try Backend API
            try
            {
                var reqObj = new
                {
                    search = search,
                    fromdate = fromDate?.ToString("yyyy-MM-dd"),
                    todate = toDate?.ToString("yyyy-MM-dd"),
                    page = 1,
                    pagesize = 100
                };
                var apiRes = await _http.PostAsJsonAsync("api/HmsDueCollection/advance/refund-history", reqObj);
                if (apiRes.IsSuccessStatusCode)
                {
                    var rRes = await apiRes.Content.ReadFromJsonAsync<HmsAdvanceRefundBackendResponse>();
                    if (rRes?.data != null && rRes.data.Count > 0)
                    {
                        return rRes.data.Select(x => new AdvanceRefundHistoryItem
                        {
                            receiptguid = x.receiptguid,
                            receiptsnoprint = x.receipt_no ?? x.receiptguid,
                            receiptdate = x.receiptdate,
                            custid = x.custid,
                            custcode = x.custcode,
                            patient_name = x.patient_name,
                            mobile = x.mobile_no,
                            refund_amount = x.refund_amount,
                            processed_by = x.enteredby_name,
                            counter_name = x.counter_name,
                            remarks = x.remarks
                        }).ToList();
                    }
                }
            }
            catch { }

            return new List<AdvanceRefundHistoryItem>();
        }

        // ─────────────────────────────────────────────────────────────────────────
        // ADVANCE REFUND REQUESTS & PROCESSING (REQUEST FLOW & CANCEL)
        // ─────────────────────────────────────────────────────────────────────────
        public async Task<(bool success, string message, AdvanceRequestItem? data)> CreateRefundRequestAsync(HmsCreateRefundRequestDto req)
        {
            if (req.custid <= 0)
                return (false, "Patient custid is required.", null);
            if (req.refund_amount <= 0)
                return (false, "Refund amount must be greater than zero.", null);

            var summary = await GetPatientAdvanceSummaryAsync(req.custid);
            double availableBal = summary?.available_balance ?? 0;
            if (req.refund_amount > availableBal)
            {
                return (false, $"Requested refund amount (₹{req.refund_amount:N2}) exceeds available advance balance (₹{availableBal:N2}).", null);
            }

            try
            {
                var response = await _http.PostAsJsonAsync("api/HmsDueCollection/advance/refund-request", req);
                if (response.IsSuccessStatusCode)
                {
                    var wrapper = await response.Content.ReadFromJsonAsync<HmsAdvanceRequestCreateBackendResponse>();
                    if (wrapper?.data != null)
                    {
                        var item = new AdvanceRequestItem
                        {
                            request_id = wrapper.data.request_id,
                            request_no = wrapper.data.request_no,
                            request_time = wrapper.data.request_date,
                            custid = wrapper.data.custid,
                            custcode = !string.IsNullOrWhiteSpace(wrapper.data.custcode) ? wrapper.data.custcode : $"CUST-{wrapper.data.custid}",
                            patient_name = wrapper.data.patient_name,
                            mobile = wrapper.data.mobile_no,
                            amount = wrapper.data.amount,
                            source_type = wrapper.data.purpose ?? "Discharge Refund",
                            notes = wrapper.data.remarks,
                            status = wrapper.data.status,
                            request_type = "REFUND"
                        };
                        return (true, "Advance refund request sent to cash counter.", item);
                    }
                }
            }
            catch { }

            var inMemItem = new AdvanceRequestItem
            {
                request_id = Guid.NewGuid(),
                request_no = $"REFR-{DateTime.Now:yyyyMMdd}-{(_advanceRequests.Count + 101):D4}",
                request_time = DateTime.UtcNow,
                custid = req.custid,
                custcode = $"CUST-{req.custid}",
                patient_name = req.patient_name ?? "",
                mobile = req.mobile_no,
                amount = req.refund_amount,
                source_type = req.reason ?? "Discharge Refund",
                notes = req.remarks,
                status = "PENDING",
                request_type = "REFUND"
            };
            lock (_advanceRequests)
            {
                _advanceRequests.Insert(0, inMemItem);
            }
            return (true, "Advance refund request sent to cash counter.", inMemItem);
        }

        public async Task<List<AdvanceRequestItem>> GetRefundRequestsAsync(string status = "PENDING", int? cntcode = null, string? tenantCode = null)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/HmsDueCollection/advance/refund-requests", new
                {
                    status = status == "ALL" ? null : status,
                    cntcode = cntcode,
                    page = 1,
                    pagesize = 50
                });
                if (response.IsSuccessStatusCode)
                {
                    var apiRes = await response.Content.ReadFromJsonAsync<HmsAdvanceRequestsBackendResponse>();
                    if (apiRes?.data != null && apiRes.data.Count > 0)
                    {
                        return apiRes.data.Select(r => new AdvanceRequestItem
                        {
                            request_id = r.request_id,
                            request_no = r.request_no,
                            request_time = r.request_date,
                            custid = r.custid,
                            custcode = string.IsNullOrWhiteSpace(r.custcode) ? $"CUST-{r.custid}" : r.custcode,
                            patient_name = r.patient_name,
                            mobile = r.mobile_no,
                            amount = r.amount,
                            source_type = r.purpose ?? "Refund",
                            notes = r.remarks,
                            status = r.status,
                            request_type = "REFUND",
                            receiptguid = r.receiptguid,
                            receiptsnoprint = r.receiptsnoprint
                        }).ToList();
                    }
                }
            }
            catch { }

            lock (_advanceRequests)
            {
                var q = _advanceRequests.Where(r => r.request_type == "REFUND" || r.request_no.StartsWith("REFR-"));
                if (!string.Equals(status, "ALL", StringComparison.OrdinalIgnoreCase))
                    q = q.Where(r => string.Equals(r.status, status, StringComparison.OrdinalIgnoreCase));
                return q.OrderByDescending(r => r.request_time).ToList();
            }
        }

        public async Task<RefundRequestsSummary> GetRefundRequestsSummaryAsync()
        {
            var summary = new RefundRequestsSummary();

            lock (_advanceRequests)
            {
                var refunds = _advanceRequests.Where(r => r.request_type == "REFUND" || r.request_no.StartsWith("REFR-")).ToList();
                summary.pending_count = refunds.Count(r => r.status == "PENDING");
                summary.pending_amount = refunds.Where(r => r.status == "PENDING").Sum(r => r.amount);
                summary.refunded_count = refunds.Count(r => r.status == "REFUNDED" || r.status == "COLLECTED");
                summary.refunded_amount = refunds.Where(r => r.status == "REFUNDED" || r.status == "COLLECTED").Sum(r => r.amount);
            }
            return summary;
        }

        public async Task<(bool success, string message, HmsAdvanceReceiptResponse? receipt)> ProcessRefundRequestAsync(HmsProcessRefundRequestDto req)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/HmsDueCollection/advance/refund-request/process", req);
                if (response.IsSuccessStatusCode)
                {
                    var wrapper = await response.Content.ReadFromJsonAsync<HmsAdvanceReceiptResponseWrapper>();
                    if (wrapper?.data != null)
                    {
                        return (true, "Advance refund processed successfully.", wrapper.data);
                    }
                }
            }
            catch { }

            return (false, "Failed to process refund request via API endpoint.", null);
        }

        public async Task<bool> CancelAnyAdvanceRequestAsync(Guid requestId, string? cancelReason = null, int? userCode = null)
        {
            try
            {
                var reqDto = new
                {
                    request_id = requestId,
                    cancel_reason = cancelReason ?? "Cancelled by user",
                    usercode = userCode
                };
                var response = await _http.PostAsJsonAsync("api/HmsDueCollection/advance/request/cancel", reqDto);
                if (response.IsSuccessStatusCode) return true;
            }
            catch { }

            lock (_advanceRequests)
            {
                var match = _advanceRequests.FirstOrDefault(r => r.request_id == requestId);
                if (match != null)
                {
                    match.status = "CANCELLED";
                    if (!string.IsNullOrWhiteSpace(cancelReason)) match.notes = (match.notes ?? "") + $" [Cancelled: {cancelReason}]";
                    return true;
                }
            }
            return false;
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

    public class HmsAdvancePatientItem
    {
        public decimal custid { get; set; }
        public string? custcode { get; set; }
        public string? patient_name { get; set; }
        public string? mobile { get; set; }
        public string? city { get; set; }
        public double total_deposited { get; set; }
        public double total_used { get; set; }
        public double total_refunded { get; set; }
        public double available_balance { get; set; }
        public DateTime? last_deposit_date { get; set; }
    }

    public class HmsAdvancePatientsResponseWrapper
    {
        public bool success { get; set; }
        public string? message { get; set; }
        public List<HmsAdvancePatientItem>? data { get; set; }
    }

    public class HmsAdvanceCancelRequest
    {
        public string receiptguid { get; set; } = string.Empty;
        public int? usercode { get; set; }
        public string? reason { get; set; }
    }

    public class HmsAdvanceCancelResponse
    {
        public string receipt_guid { get; set; } = string.Empty;
        public string? receipt_no { get; set; }
        public decimal? custid { get; set; }
        public double cancelled_amount { get; set; }
        public double advance_balance_before { get; set; }
        public double advance_balance_after { get; set; }
        public DateTime cancelled_date { get; set; }
    }

    public class HmsAdvanceCancelResponseWrapper
    {
        public bool success { get; set; }
        public string? message { get; set; }
        public HmsAdvanceCancelResponse? data { get; set; }
    }

    public class AdvanceRequestItem
    {
        public Guid request_id { get; set; }
        public string request_no { get; set; } = string.Empty;
        public DateTime request_time { get; set; } = DateTime.UtcNow;
        public decimal custid { get; set; }
        public string custcode { get; set; } = string.Empty;
        public string patient_name { get; set; } = string.Empty;
        public string? mobile { get; set; }
        public double amount { get; set; }
        public string source_type { get; set; } = "Billing";
        public string? notes { get; set; }
        public string status { get; set; } = "PENDING"; // PENDING, RECEIVED, CANCELLED
        public string request_type { get; set; } = "ADVANCE";
        public string? payment_mode { get; set; }
        public string? reference_no { get; set; }
        public int? target_cntcode { get; set; }
        public int? requested_by { get; set; }
        public string? requested_by_name { get; set; }
        public string tenant_code { get; set; } = "DEFAULT";
        public string? receiptguid { get; set; }
        public string? receiptsnoprint { get; set; }
    }

    public class HmsAdvanceRequestsBackendResponse
    {
        public bool success { get; set; }
        public int totalCount { get; set; }
        public int page { get; set; }
        public int pagesize { get; set; }
        public HmsAdvanceRequestListSummary? summary { get; set; }
        public List<HmsAdvanceRequestRow>? data { get; set; }
    }

    public class HmsAdvanceRequestCreateBackendResponse
    {
        public bool success { get; set; }
        public string? message { get; set; }
        public HmsAdvanceRequestRow? data { get; set; }
    }

    public class HmsAdvanceRequestRow
    {
        public Guid request_id { get; set; }
        public string request_no { get; set; } = string.Empty;
        public DateTime request_date { get; set; } = DateTime.UtcNow;
        public decimal custid { get; set; }
        public string custcode { get; set; } = string.Empty;
        public string patient_name { get; set; } = string.Empty;
        public string? mobile_no { get; set; }
        public string? opvisitid { get; set; }
        public Guid? ip_id { get; set; }
        public double amount { get; set; }
        public string? purpose { get; set; }
        public string? remarks { get; set; }
        public string status { get; set; } = "PENDING";
        public string? request_type { get; set; }
        public string? payment_mode { get; set; }
        public string? reference_no { get; set; }
        public int? requested_by { get; set; }
        public string? requested_by_name { get; set; }
        public int? target_cntcode { get; set; }
        public int? target_bhcode { get; set; }
        public int? collected_by { get; set; }
        public string? collected_by_name { get; set; }
        public DateTime? collected_date { get; set; }
        public string? receiptguid { get; set; }
        public string? receiptsnoprint { get; set; }
        public string? cancel_reason { get; set; }
        public int? cancelled_by { get; set; }
        public DateTime? cancelled_date { get; set; }
        public string tenant_code { get; set; } = string.Empty;
    }

    public class HmsAdvanceRequestListSummary
    {
        public int total_requests { get; set; }
        public int pending_count { get; set; }
        public double pending_amount { get; set; }
        public int collected_count { get; set; }
        public double collected_amount { get; set; }
        public int cancelled_count { get; set; }
        public double cancelled_amount { get; set; }
    }

    public class HmsPatientAdvanceDepositBackendResponse
    {
        public bool success { get; set; }
        public int totalCount { get; set; }
        public int page { get; set; }
        public int pagesize { get; set; }
        public HmsPatientAdvanceDepositSummary? summary { get; set; }
        public List<HmsPatientAdvanceDepositRow>? data { get; set; }
    }

    public class HmsPatientAdvanceDepositSummary
    {
        public int total_patients { get; set; }
        public double total_deposited { get; set; }
        public double total_used { get; set; }
        public double total_refunded { get; set; }
        public double total_available_balance { get; set; }
    }

    public class HmsPatientAdvanceDepositRow
    {
        public decimal custid { get; set; }
        public string custcode { get; set; } = string.Empty;
        public string patient_name { get; set; } = string.Empty;
        public string mobile_no { get; set; } = string.Empty;
        public string gender { get; set; } = string.Empty;
        public int age { get; set; }
        public double total_deposited { get; set; }
        public double total_used { get; set; }
        public double total_refunded { get; set; }
        public double available_balance { get; set; }
        public DateTime? first_deposit_date { get; set; }
        public DateTime? last_deposit_date { get; set; }
        public string? last_receipt_no { get; set; }
        public int deposit_count { get; set; }
        public string status { get; set; } = "ACTIVE";
    }

    public class HmsAdvanceRefundBackendResponse
    {
        public bool success { get; set; }
        public int totalCount { get; set; }
        public int page { get; set; }
        public int pagesize { get; set; }
        public HmsAdvanceRefundSummary? summary { get; set; }
        public List<HmsAdvanceRefundHistoryRow>? data { get; set; }
    }

    public class HmsAdvanceRefundSummary
    {
        public int total_refunds { get; set; }
        public double total_refund_amount { get; set; }
    }

    public class HmsAdvanceRefundHistoryRow
    {
        public string receiptguid { get; set; } = string.Empty;
        public string? receipt_no { get; set; }
        public string? receiptbarcode { get; set; }
        public DateTime receiptdate { get; set; }
        public decimal custid { get; set; }
        public string custcode { get; set; } = string.Empty;
        public string patient_name { get; set; } = string.Empty;
        public string? mobile_no { get; set; }
        public double refund_amount { get; set; }
        public string? payment_mode { get; set; }
        public string? enteredby_name { get; set; }
        public string? counter_name { get; set; }
        public string? remarks { get; set; }
    }

    public class AdvanceRequestsSummary
    {
        public int pending_count { get; set; }
        public double pending_amount { get; set; }
        public int received_count { get; set; }
        public double received_amount { get; set; }
    }

    public class PatientDepositsSummary
    {
        public double total_deposited_sum { get; set; }
        public double total_used_sum { get; set; }
        public double total_refunded_sum { get; set; }
        public double total_available_sum { get; set; }
    }

    public class AdvanceRefundHistoryItem
    {
        public string receiptguid { get; set; } = string.Empty;
        public string receiptsnoprint { get; set; } = string.Empty;
        public DateTime? receiptdate { get; set; }
        public decimal custid { get; set; }
        public string custcode { get; set; } = string.Empty;
        public string patient_name { get; set; } = string.Empty;
        public string? mobile { get; set; }
        public double refund_amount { get; set; }
        public string? processed_by { get; set; }
        public string? counter_name { get; set; }
        public string? remarks { get; set; }
    }
}
    public class HmsCreateRefundRequestDto
    {
        public decimal custid { get; set; }
        public string? patient_name { get; set; }
        public string? mobile_no { get; set; }
        public string? opvisitid { get; set; }
        public Guid? ip_id { get; set; }
        public double refund_amount { get; set; }
        public string? reason { get; set; }
        public string? remarks { get; set; }
        public int? target_cntcode { get; set; }
        public int? target_bhcode { get; set; }
        public int? usercode { get; set; }
        public string? user_name { get; set; }
        public string? tenant_code { get; set; }
    }

    public class HmsProcessRefundRequestDto
    {
        public Guid request_id { get; set; }
        public double? amount { get; set; }
        public string payment_mode { get; set; } = "CASH";
        public string? reference_no { get; set; }
        public int? enteredbhcode { get; set; }
        public int? cntcode { get; set; }
        public int? usercode { get; set; }
        public string? user_name { get; set; }
        public int? computercode { get; set; }
        public string? remarks { get; set; }
    }

    public class RefundRequestsSummary
    {
        public int pending_count { get; set; }
        public double pending_amount { get; set; }
        public int refunded_count { get; set; }
        public double refunded_amount { get; set; }
    }
