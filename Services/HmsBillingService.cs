using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Booking.Models;
using Booking.Helpers;
using Medico_Backend.Model;
using SharedComponents.Rcl.Services;

namespace Booking.Services
{
    public class HmsBillingService
    {
        private readonly HttpClient _http;
        private readonly IHttpClientFactory? _clientFactory;
        private readonly TenantSessionState? _session;

        public HmsBillingService(HttpClient http, IHttpClientFactory? clientFactory = null, TenantSessionState? session = null)
        {
            _http = http;
            _clientFactory = clientFactory;
            _session = session;
        }

        private HttpClient GetClient(string? tenantCode = null)
        {
            var effectiveTenant = !string.IsNullOrWhiteSpace(tenantCode) ? tenantCode : _session?.TenantCode;
            if (_clientFactory != null)
            {
                var client = _clientFactory.CreateClient("DoctorApi");
                client.DefaultRequestHeaders.Remove("tenantcode");
                client.DefaultRequestHeaders.Remove("tenant_code");
                client.DefaultRequestHeaders.Remove("tenant-code");
                if (!string.IsNullOrEmpty(effectiveTenant))
                {
                    client.DefaultRequestHeaders.Add("tenantcode", effectiveTenant);
                    client.DefaultRequestHeaders.Add("tenant_code", effectiveTenant);
                    client.DefaultRequestHeaders.Add("tenant-code", effectiveTenant);
                }
                if (!string.IsNullOrEmpty(_session?.AuthToken))
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.AuthToken);
                }
                return client;
            }
            return _http;
        }

        public async Task<string> SaveBillAsync(HmsBillModel bill)
        {
            try
            {
                var jsonPayload = System.Text.Json.JsonSerializer.Serialize(bill, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine("--- BILL JSON PAYLOAD ---");
                Console.WriteLine(jsonPayload);
                Console.WriteLine("-------------------------");

                var response = await _http.PostAsJsonAsync("api/HmsBilling/save-bill", bill);
                var rawResponse = await response.Content.ReadAsStringAsync();
                
                Console.WriteLine($"--- API RESPONSE: {rawResponse} ---");
                
                return rawResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving bill: {ex.Message}");
                return $"Error|{ex.Message}";
            }
        }

        public async Task<HmsBillModel?> GetBillAsync(Guid opId)
        {
            try
            {
                // First try standard get-bill endpoint
                var response = await _http.GetAsync($"api/HmsBilling/get-bill?op_id={opId}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<HmsBillModel>();
                }
                
                // Fallback to searching by op_id under a list endpoint if any, or return null
                var fallbackResponse = await _http.GetAsync($"api/HmsBilling/by-op?op_id={opId}");
                if (fallbackResponse.IsSuccessStatusCode)
                {
                    return await fallbackResponse.Content.ReadFromJsonAsync<HmsBillModel>();
                }
                
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching bill: {ex.Message}");
                return null;
            }
        }

        public async Task<HmsBillListResponse?> ListBillsAsync(HmsBillFilterRequest filter)
        {
            try
            {
                var sanitizedFilter = new HmsBillFilterRequest
                {
                    bhcode = filter.bhcode.HasValue && filter.bhcode.Value > 0 ? filter.bhcode.Value : null,
                    cntcode = filter.cntcode.HasValue && filter.cntcode.Value > 0 ? filter.cntcode.Value : null,
                    ip_id = filter.ip_id,
                    fromdate = filter.fromdate.HasValue ? filter.fromdate.Value.Date : null,
                    todate = filter.todate.HasValue ? filter.todate.Value.Date.AddDays(1).AddSeconds(-1) : null,
                    custid = filter.custid,
                    dcode = filter.dcode,
                    pendingonly = filter.pendingonly,
                    iscashbill = filter.iscashbill,
                    iscreditbill = filter.iscreditbill,
                    search = filter.search,
                    page = filter.page,
                    pagesize = filter.pagesize
                };

                Console.WriteLine($"[ListBillsAsync] Request: from={sanitizedFilter.fromdate:yyyy-MM-dd HH:mm:ss}, to={sanitizedFilter.todate:yyyy-MM-dd HH:mm:ss}, bh={sanitizedFilter.bhcode}, pending={sanitizedFilter.pendingonly}, search={sanitizedFilter.search}");

                var response = await _http.PostAsJsonAsync("api/HmsBilling/list-bills", sanitizedFilter);
                var rawResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[ListBillsAsync] Response ({response.StatusCode}): {rawResponse}");

                if (response.IsSuccessStatusCode)
                {
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<HmsBillListResponse>(rawResponse);
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching bills list: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> AddPaymentAsync(HmsPaymentRequest request)
        {
            try
            {
                // Sanitize paymode codes: if 0, convert to null
                if (request.pmc1.HasValue && request.pmc1.Value == 0) request.pmc1 = null;
                if (request.pmc2.HasValue && request.pmc2.Value == 0) request.pmc2 = null;
                if (request.pmc3.HasValue && request.pmc3.Value == 0) request.pmc3 = null;

                // Zero out amount if corresponding pmc is null
                if (!request.pmc1.HasValue) request.pmc1_amount = null;
                if (!request.pmc2.HasValue) request.pmc2_amount = null;
                if (!request.pmc3.HasValue) request.pmc3_amount = null;

                // Ensure reference_no and bank_name are non-null strings
                request.reference_no ??= "";
                request.bank_name ??= "";

                var response = await _http.PostAsJsonAsync("api/HmsBilling/add-payment", request);
                if (!response.IsSuccessStatusCode)
                {
                    var errText = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[AddPaymentAsync] Failed ({response.StatusCode}): {errText}");
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding payment: {ex.Message}");
                return false;
            }
        }

        public async Task<HmsBillResponse?> GetBillByGuidAsync(string guid)
        {
            try
            {
                var response = await _http.GetAsync($"api/HmsBilling/get-bill/{guid}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<HmsBillResponse>();
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching bill by guid: {ex.Message}");
                return null;
            }
        }
        public async Task<string> UpdateBillAsync(HmsBillModel model)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/HmsBilling/update-bill", model);
                var rawResponse = await response.Content.ReadAsStringAsync();
                
                Console.WriteLine($"--- API RESPONSE (UPDATE): {rawResponse} ---");
                
                return rawResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating bill: {ex.Message}");
                return $"Error|{ex.Message}";
            }
        }

        public async Task<string> CancelBillAsync(string requestguid, int usercode, string reason)
        {
            try
            {
                var payload = new { requestguid, usercode, reason };
                var response = await _http.PostAsJsonAsync("api/HmsBilling/cancel-bill", payload);
                var rawResponse = await response.Content.ReadAsStringAsync();
                
                Console.WriteLine($"--- API RESPONSE (CANCEL): {rawResponse} ---");
                
                return rawResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cancelling bill: {ex.Message}");
                return $"Error|{ex.Message}";
            }
        }

        public async Task<LabSettingModel?> GetLabSettingInternalAsync()
        {
            try
            {
                var client = GetClient();
                var rawJson = await client.GetStringAsync("api/LabSetting/get");
                if (string.IsNullOrWhiteSpace(rawJson)) return null;

                using var doc = System.Text.Json.JsonDocument.Parse(rawJson);
                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    var list = System.Text.Json.JsonSerializer.Deserialize<List<LabSettingModel>>(rawJson, options);
                    return list?.FirstOrDefault();
                }
                else if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Object)
                {
                    if (doc.RootElement.TryGetProperty("data", out var dataProp) && dataProp.ValueKind == System.Text.Json.JsonValueKind.Array)
                    {
                        var list = System.Text.Json.JsonSerializer.Deserialize<List<LabSettingModel>>(dataProp.GetRawText(), options);
                        return list?.FirstOrDefault();
                    }
                    if (doc.RootElement.TryGetProperty("value", out var valProp) && valProp.ValueKind == System.Text.Json.JsonValueKind.Array)
                    {
                        var list = System.Text.Json.JsonSerializer.Deserialize<List<LabSettingModel>>(valProp.GetRawText(), options);
                        return list?.FirstOrDefault();
                    }
                    return System.Text.Json.JsonSerializer.Deserialize<LabSettingModel>(rawJson, options);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HmsBillingService] Error fetching LabSetting: {ex.Message}");
            }
            return null;
        }

        public static bool TryParseShiftEndTime(string? timeStr, out TimeSpan timeSpan)
        {
            timeSpan = TimeSpan.Zero;
            if (string.IsNullOrWhiteSpace(timeStr)) return false;
            timeStr = timeStr.Trim();

            if (TimeSpan.TryParse(timeStr, out timeSpan))
            {
                return true;
            }

            if (TimeOnly.TryParse(timeStr, out var timeOnly))
            {
                timeSpan = timeOnly.ToTimeSpan();
                return true;
            }

            if (DateTime.TryParse(timeStr, out var parsedDt))
            {
                timeSpan = parsedDt.TimeOfDay;
                return true;
            }

            return false;
        }

        public async Task<List<CounterTimingDto>> GetAllRawUnclosedCountersAsync()
        {
            try
            {
                var client = GetClient();
                var rawJson = await client.GetStringAsync("api/CounterTiming/get");
                if (!string.IsNullOrWhiteSpace(rawJson))
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(rawJson);
                    var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    List<CounterTimingDto>? list = null;
                    if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                    {
                        list = System.Text.Json.JsonSerializer.Deserialize<List<CounterTimingDto>>(rawJson, options);
                    }
                    else if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Object)
                    {
                        if (doc.RootElement.TryGetProperty("data", out var dataProp))
                            list = System.Text.Json.JsonSerializer.Deserialize<List<CounterTimingDto>>(dataProp.GetRawText(), options);
                        else if (doc.RootElement.TryGetProperty("value", out var valProp))
                            list = System.Text.Json.JsonSerializer.Deserialize<List<CounterTimingDto>>(valProp.GetRawText(), options);
                    }

                    if (list != null)
                    {
                        return list.Where(s => s.todate == null).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetAllRawUnclosedCountersAsync] Error: {ex.Message}");
            }
            return new List<CounterTimingDto>();
        }

        public async Task<bool> ForceCloseCounterShiftAsync(Guid cnttid, DateTime? todate = null, string closeType = "AUTO")
        {
            try
            {
                var client = GetClient();
                var effectiveToDate = todate ?? DateTime.UtcNow;

                // 1. Fetch raw counter record to preserve metadata
                var rawJson = await client.GetStringAsync("api/CounterTiming/get");
                if (!string.IsNullOrWhiteSpace(rawJson))
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(rawJson);
                    var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    List<CounterTimingDto>? list = null;
                    if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                    {
                        list = System.Text.Json.JsonSerializer.Deserialize<List<CounterTimingDto>>(rawJson, options);
                    }
                    else if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Object)
                    {
                        if (doc.RootElement.TryGetProperty("data", out var dataProp))
                            list = System.Text.Json.JsonSerializer.Deserialize<List<CounterTimingDto>>(dataProp.GetRawText(), options);
                        else if (doc.RootElement.TryGetProperty("value", out var valProp))
                            list = System.Text.Json.JsonSerializer.Deserialize<List<CounterTimingDto>>(valProp.GetRawText(), options);
                    }

                    var target = list?.FirstOrDefault(s => s.cnttid == cnttid);
                    if (target != null)
                    {
                        var updateModel = new CounterTimingModel
                        {
                            cnttid = target.cnttid.ToString(),
                            bhcode = target.bhcode,
                            cntcode = target.cntcode,
                            shiftsno = target.shiftsno ?? 1,
                            counterdate = target.counterdate ?? target.fromdate ?? DateTime.UtcNow.Date,
                            fromdate = target.fromdate ?? target.counterdate ?? DateTime.UtcNow,
                            todate = effectiveToDate,
                            shift_mode = target.shift_mode,
                            planned_to = target.planned_to,
                            close_type = closeType,
                            tenant_code = target.tenant_code ?? _session?.TenantCode,
                            usercode = target.usercode ?? 1,
                            computercode = target.computercode ?? 1,
                            entereddate = target.entereddate ?? target.fromdate ?? DateTime.UtcNow,
                            ibsdate = target.ibsdate ?? DateTime.UtcNow
                        };

                        var updateResp = await client.PostAsJsonAsync("api/CounterTiming/update", updateModel);
                        var updateRaw = await updateResp.Content.ReadAsStringAsync();
                        Console.WriteLine($"[ForceCloseCounterShiftAsync] Update status: {updateResp.StatusCode}, response: {updateRaw}");
                        if (updateResp.IsSuccessStatusCode)
                        {
                            Console.WriteLine($"[ForceCloseCounterShiftAsync] Successfully closed shift {cnttid} via api/CounterTiming/update (todate={effectiveToDate:o})");
                            return true;
                        }
                    }
                }

                // 2. Direct delete fallback if update not possible or shift needs freeing
                var delResp = await client.GetAsync($"api/CounterTiming/delete?cnttid={cnttid}");
                if (delResp.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[ForceCloseCounterShiftAsync] Successfully freed counter for shift {cnttid} via api/CounterTiming/delete");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ForceCloseCounterShiftAsync] Exception: {ex.Message}");
            }
            return false;
        }

        public async Task<bool> DeleteCounterTimingAsync(Guid cnttid)
        {
            try
            {
                var client = GetClient();
                var delResp = await client.GetAsync($"api/CounterTiming/delete?cnttid={cnttid}");
                return delResp.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DeleteCounterTimingAsync] Error: {ex.Message}");
                return false;
            }
        }

        public async Task<string> OpenShiftAsync(OpenShiftRequest request)
        {
            try
            {
                var setting = await GetLabSettingInternalAsync();
                var nowIndian = DateTime.UtcNow.ToIndianTime();

                if (setting != null && string.Equals(setting.shift_timing_mode, "FIXED", StringComparison.OrdinalIgnoreCase))
                {
                    request.shift_mode = "FIXED";
                    if (TryParseShiftEndTime(setting.shift_end_time, out var endTime))
                    {
                        var fixedEndDateTime = nowIndian.Date.Add(endTime);
                        // If opened before today's fixed shift end time -> ends at the fixed time
                        if (nowIndian < fixedEndDateTime)
                        {
                            request.planned_to = fixedEndDateTime.ToUtcFromIndianTime();
                        }
                        else
                        {
                            // Newly created shift opened after the fixed time -> ends at 12:00 AM (midnight)
                            var midnight = nowIndian.Date.AddDays(1).Date;
                            request.planned_to = midnight.ToUtcFromIndianTime();
                        }
                    }
                    else
                    {
                        // If no fixed timing is set, the shift should close at 12:00 AM midnight
                        var midnight = nowIndian.Date.AddDays(1).Date;
                        request.planned_to = midnight.ToUtcFromIndianTime();
                    }
                }
                else
                {
                    // Variable shift mode -> ends at 12:00 AM midnight (or when manually closed)
                    request.shift_mode = "VARIABLE";
                    var midnight = nowIndian.Date.AddDays(1).Date;
                    request.planned_to = midnight.ToUtcFromIndianTime();
                }

                // Clean up any stale unclosed shift for this specific counter before opening new shift
                try
                {
                    var unclosed = await GetAllRawUnclosedCountersAsync();
                    var existingForCounter = unclosed.Where(s => s.cntcode == request.cntcode && (request.bhcode == 0 || s.bhcode == request.bhcode || s.bhcode == null)).ToList();
                    foreach (var oldShift in existingForCounter)
                    {
                        Console.WriteLine($"[OpenShiftAsync] Proactively clearing old shift {oldShift.cnttid} on Counter {request.cntcode}");
                        var closed = await ForceCloseCounterShiftAsync(oldShift.cnttid, oldShift.planned_to ?? DateTime.UtcNow, "AUTO");
                        if (!closed)
                        {
                            await DeleteCounterTimingAsync(oldShift.cnttid);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[OpenShiftAsync] Pre-close check notice: {ex.Message}");
                }

                var client = GetClient();
                var jsonPayload = System.Text.Json.JsonSerializer.Serialize(request, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine("--- OPEN SHIFT JSON PAYLOAD ---");
                Console.WriteLine(jsonPayload);

                var response = await client.PostAsJsonAsync("api/HmsBilling/counter/open-shift", request);
                var rawResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine("--- OPEN SHIFT API RESPONSE ---");
                Console.WriteLine(rawResponse);

                // If backend returned active shift conflict, force-close the blocker and retry
                if (rawResponse.Contains("operational", StringComparison.OrdinalIgnoreCase) ||
                    rawResponse.Contains("active shift counter", StringComparison.OrdinalIgnoreCase) ||
                    rawResponse.Contains("already open", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("[OpenShiftAsync] Backend indicated active shift conflict. Forcing cleanup of blocking shift and re-attempting open...");
                    try
                    {
                        var unclosedAfter = await GetAllRawUnclosedCountersAsync();
                        var blockers = unclosedAfter.Where(s => s.cntcode == request.cntcode).ToList();
                        foreach (var blocker in blockers)
                        {
                            var closed = await ForceCloseCounterShiftAsync(blocker.cnttid, DateTime.UtcNow, "AUTO");
                            if (!closed)
                            {
                                await DeleteCounterTimingAsync(blocker.cnttid);
                            }
                        }

                        var retryResponse = await client.PostAsJsonAsync("api/HmsBilling/counter/open-shift", request);
                        var retryRaw = await retryResponse.Content.ReadAsStringAsync();
                        Console.WriteLine("--- OPEN SHIFT RETRY API RESPONSE ---");
                        Console.WriteLine(retryRaw);
                        return retryRaw;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[OpenShiftAsync] Retry cleanup notice: {ex.Message}");
                    }
                }

                return rawResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening shift: {ex.Message}");
                return $"Error|{ex.Message}";
            }
        }

        public async Task<string> CloseShiftAsync(CloseShiftRequest request)
        {
            try
            {
                LabSettingModel? labSetting = await GetLabSettingInternalAsync();

                bool isFixedMode = string.Equals(labSetting?.shift_timing_mode, "FIXED", StringComparison.OrdinalIgnoreCase)
                                   || string.Equals(request.close_type, "FIXED", StringComparison.OrdinalIgnoreCase);

                if (isFixedMode && !string.Equals(request.close_type, "AUTO", StringComparison.OrdinalIgnoreCase))
                {
                    return "{\"message\": \"The fixed timing shift cannot be closed manually. It will close automatically at the configured time or at midnight (12:00 AM).\", \"status\": \"warning\"}";
                }

                if (string.IsNullOrWhiteSpace(request.close_type))
                {
                    request.close_type = !string.IsNullOrWhiteSpace(labSetting?.shift_timing_mode)
                        ? labSetting.shift_timing_mode
                        : "VARIABLE";
                }

                if (!request.todate.HasValue)
                {
                    request.todate = DateTime.UtcNow;
                }

                var client = GetClient();
                var jsonPayload = System.Text.Json.JsonSerializer.Serialize(request, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine("--- CLOSE SHIFT JSON PAYLOAD ---");
                Console.WriteLine(jsonPayload);

                var response = await client.PostAsJsonAsync("api/HmsBilling/counter/close-shift", request);
                var rawResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"--- CLOSE SHIFT API RESPONSE ({response.StatusCode}): {rawResponse} ---");

                if (response.IsSuccessStatusCode)
                {
                    return rawResponse;
                }

                // If backend close-shift rejected (in variable mode), fallback to direct counter update
                if (Guid.TryParse(request.cnttid, out var parsedGuid))
                {
                    Console.WriteLine($"[CloseShiftAsync] Standard close-shift returned {response.StatusCode}. Attempting direct counter close fallback for {parsedGuid}...");
                    var closed = await ForceCloseCounterShiftAsync(parsedGuid, request.todate, request.close_type ?? "MANUAL");
                    if (closed)
                    {
                        return "{\"message\": \"Counter shift safely shut down.\", \"status\": \"success\"}";
                    }
                }

                return rawResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error closing shift: {ex.Message}");
                if (Guid.TryParse(request.cnttid, out var parsedGuid))
                {
                    try
                    {
                        var closed = await ForceCloseCounterShiftAsync(parsedGuid, request.todate, request.close_type ?? "MANUAL");
                        if (closed)
                        {
                            return "{\"message\": \"Counter shift safely shut down.\", \"status\": \"success\"}";
                        }
                    }
                    catch { }
                }
                return $"Error|{ex.Message}";
            }
        }

        public async Task<BillNoListResponse?> ListBillNoConfigsAsync(BillNoListRequest request)
        {
            try
            {
                var response = await GetClient().PostAsJsonAsync("api/HmsBilling/billno/list", request);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<BillNoListResponse>();
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching bill no configs list: {ex.Message}");
                return null;
            }
        }

        public async Task<BillNoConfig?> GetBillNoConfigAsync(int bncode)
        {
            try
            {
                var response = await GetClient().GetAsync($"api/HmsBilling/billno/{bncode}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<BillNoConfig>();
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching bill no config: {ex.Message}");
                return null;
            }
        }

        public async Task<string> CreateBillNoConfigAsync(BillNoConfig model)
        {
            try
            {
                var response = await GetClient().PostAsJsonAsync("api/HmsBilling/billno/create", model);
                var rawResponse = await response.Content.ReadAsStringAsync();
                return rawResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating bill no config: {ex.Message}");
                return $"Error|{ex.Message}";
            }
        }

        public async Task<string> UpdateBillNoConfigAsync(BillNoConfig model)
        {
            try
            {
                var response = await GetClient().PostAsJsonAsync("api/HmsBilling/billno/update", model);
                var rawResponse = await response.Content.ReadAsStringAsync();
                return rawResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating bill no config: {ex.Message}");
                return $"Error|{ex.Message}";
            }
        }

        public async Task<string> DeleteBillNoConfigAsync(int bncode, int usercode)
        {
            try
            {
                var payload = new BillNoDeleteRequest { bncode = bncode, usercode = usercode };
                var response = await GetClient().PostAsJsonAsync("api/HmsBilling/billno/delete", payload);
                var rawResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"--- BillNo Delete Response (bncode={bncode}) ---");
                Console.WriteLine(rawResponse);
                Console.WriteLine("------------------------------------------------");
                return rawResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting bill no config: {ex.Message}");
                return $"Error|{ex.Message}";
            }
        }

        public async Task<List<CounterTimingDto>> GetOpenCountersAsync(bool returnAllUnclosed = false, string? tenantCode = null)
        {
            try
            {
                var effectiveTenant = !string.IsNullOrWhiteSpace(tenantCode) ? tenantCode : _session?.TenantCode;
                List<HttpClient> clientsToTry = new();
                clientsToTry.Add(GetClient(effectiveTenant));
                if (_clientFactory != null)
                {
                    try
                    {
                        var labClient = _clientFactory.CreateClient("LabCareUrl");
                        labClient.DefaultRequestHeaders.Remove("tenant_code");
                        labClient.DefaultRequestHeaders.Remove("tenantcode");
                        if (!string.IsNullOrEmpty(effectiveTenant))
                        {
                            labClient.DefaultRequestHeaders.Add("tenant_code", effectiveTenant);
                            labClient.DefaultRequestHeaders.Add("tenantcode", effectiveTenant);
                        }
                        if (!string.IsNullOrEmpty(_session?.AuthToken))
                        {
                            labClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.AuthToken);
                        }
                        clientsToTry.Add(labClient);
                    }
                    catch { }
                }
                if (!clientsToTry.Contains(_http))
                {
                    clientsToTry.Add(_http);
                }

                List<CounterTimingDto>? rawCounters = null;
                string[] endpoints = new[] { "api/CounterTiming/get", "CounterTiming/get" };
                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                foreach (var client in clientsToTry)
                {
                    foreach (var ep in endpoints)
                    {
                        try
                        {
                            var response = await client.GetAsync(ep);
                            if (response.IsSuccessStatusCode)
                            {
                                var rawJson = await response.Content.ReadAsStringAsync();
                                if (!string.IsNullOrWhiteSpace(rawJson))
                                {
                                    using var doc = System.Text.Json.JsonDocument.Parse(rawJson);
                                    if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                                    {
                                        rawCounters = System.Text.Json.JsonSerializer.Deserialize<List<CounterTimingDto>>(rawJson, options);
                                    }
                                    else if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Object)
                                    {
                                        if (doc.RootElement.TryGetProperty("data", out var dataProp))
                                            rawCounters = System.Text.Json.JsonSerializer.Deserialize<List<CounterTimingDto>>(dataProp.GetRawText(), options);
                                        else if (doc.RootElement.TryGetProperty("value", out var valProp))
                                            rawCounters = System.Text.Json.JsonSerializer.Deserialize<List<CounterTimingDto>>(valProp.GetRawText(), options);
                                    }

                                    if (rawCounters != null && rawCounters.Any())
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                        catch { }
                    }
                    if (rawCounters != null && rawCounters.Any()) break;
                }

                if (rawCounters != null && rawCounters.Any())
                {
                    var unclosedShifts = rawCounters.Where(s => s.todate == null).ToList();
                    if (returnAllUnclosed)
                    {
                        return unclosedShifts;
                    }

                    LabSettingModel? labSetting = await GetLabSettingInternalAsync();

                    var nowUtc = DateTime.UtcNow;
                    var nowIndian = nowUtc.ToIndianTime();

                    var activeShifts = new List<CounterTimingDto>();

                    foreach (var shift in unclosedShifts)
                    {
                        bool isExpired = IsShiftExpired(shift, labSetting, nowIndian, nowUtc);
                        if (!isExpired)
                        {
                            activeShifts.Add(shift);
                        }
                    }

                    return activeShifts;
                }
                return new List<CounterTimingDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching open counters: {ex.Message}");
                return new List<CounterTimingDto>();
            }
        }

        public static bool IsShiftExpired(CounterTimingDto shift, LabSettingModel? setting, DateTime nowIndian, DateTime nowUtc)
        {
            if (shift == null) return true;
            if (shift.todate != null) return true;

            var shiftFromIndian = (shift.fromdate ?? shift.counterdate ?? nowIndian).ToIndianTime();
            var shiftOpenDateIndian = shiftFromIndian.Date;

            // 1. Check if planned_to is set on shift record
            if (shift.planned_to.HasValue)
            {
                var planned = shift.planned_to.Value;
                var plannedUtc = planned.Kind == DateTimeKind.Utc ? planned : DateTime.SpecifyKind(planned, DateTimeKind.Utc);
                var plannedIndian = planned.ToIndianTime();

                if (nowUtc >= plannedUtc || nowIndian >= plannedIndian)
                {
                    return true;
                }
            }

            // 2. Check if LabSetting or shift is configured in FIXED mode
            string shiftMode = shift.shift_mode ?? setting?.shift_timing_mode ?? "VARIABLE";
            if (string.Equals(shiftMode, "FIXED", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(setting?.shift_timing_mode, "FIXED", StringComparison.OrdinalIgnoreCase))
            {
                string? endTimeStr = setting?.shift_end_time;
                if (TryParseShiftEndTime(endTimeStr, out var endTime))
                {
                    var fixedEndDateTime = shiftOpenDateIndian.Add(endTime);

                    if (shiftFromIndian < fixedEndDateTime)
                    {
                        // Shift opened before fixed end time -> ends at the fixed time
                        if (nowIndian >= fixedEndDateTime)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        // Newly created shift opened at or after fixed end time -> ends at 12:00 AM midnight
                        var midnight = shiftOpenDateIndian.AddDays(1).Date;
                        if (nowIndian >= midnight)
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    // If no fixed timing is set, the shift should close at 12:00 AM midnight
                    var midnight = shiftOpenDateIndian.AddDays(1).Date;
                    if (nowIndian >= midnight)
                    {
                        return true;
                    }
                }
            }
            else
            {
                // 3. Variable mode: ends at 12:00 AM midnight of the shift day
                var universalMidnight = shiftOpenDateIndian.AddDays(1).Date;
                if (nowIndian >= universalMidnight)
                {
                    return true;
                }
            }

            return false;
        }

        public async Task<CounterTimingDto?> GetOpenShiftAsync(int bhcode, int cntcode)
        {
            try
            {
                var openCounters = await GetOpenCountersAsync();
                return openCounters.FirstOrDefault(s => s.cntcode == cntcode && (bhcode == 0 || s.bhcode == null || s.bhcode == 0 || s.bhcode == bhcode));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching open shift for BH {bhcode}, Cnt {cntcode}: {ex.Message}");
                return null;
            }
        }

        public async Task<List<UnbilledChargeSummary>> GetUnbilledChargesByVisitAsync(string? opvisitid = null, Guid? ip_id = null)
        {
            try
            {
                var queryParams = new List<string>();
                if (ip_id.HasValue && ip_id.Value != Guid.Empty)
                {
                    queryParams.Add($"ip_id={ip_id.Value}");
                }
                else if (!string.IsNullOrEmpty(opvisitid))
                {
                    queryParams.Add($"opvisitid={Uri.EscapeDataString(opvisitid)}");
                }

                var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
                var rawJson = await _http.GetStringAsync($"api/UnbilledCharges/by-visit{queryString}");
                return ParseUnbilledChargeSummaryList(rawJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching unbilled charges: {ex.Message}");
                return new List<UnbilledChargeSummary>();
            }
        }

        public async Task<List<UnbilledChargeSummary>> GetAllUnbilledChargesAsync(string? op_id = null, string? ip_id = null)
        {
            try
            {
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(ip_id))
                {
                    queryParams.Add($"ip_id={Uri.EscapeDataString(ip_id)}");
                }
                if (!string.IsNullOrEmpty(op_id))
                {
                    queryParams.Add($"op_id={Uri.EscapeDataString(op_id)}");
                }

                var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
                var rawJson = await _http.GetStringAsync($"api/UnbilledCharges/get-all{queryString}");
                return ParseUnbilledChargeSummaryList(rawJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching all unbilled charges: {ex.Message}");
                return new List<UnbilledChargeSummary>();
            }
        }

        public async Task<string> AddUnbilledConsultationAsync(AddUnbilledConsultationRequest request)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/UnbilledCharges/add-consultation", request);
                var rawResponse = await response.Content.ReadAsStringAsync();
                return rawResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding unbilled consultation: {ex.Message}");
                return $"Error|{ex.Message}";
            }
        }

        public async Task<List<UnbilledChargeSummary>> GetIpRoomRentSummaryAsync(Guid ipId)
        {
            try
            {
                var rawJson = await _http.GetStringAsync($"api/UnbilledCharges/ip-room-rent-summary?ip_id={ipId}");
                return ParseUnbilledChargeSummaryList(rawJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching IP room rent summary: {ex.Message}");
                return new List<UnbilledChargeSummary>();
            }
        }

        public async Task<bool> UpdateUnbilledChargeAsync(UpdateUnbilledChargeRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.unbilledid))
                {
                    Console.WriteLine("[UpdateUnbilledCharge] Error: unbilledid is missing or request is null.");
                    return false;
                }

                var options = new System.Text.Json.JsonSerializerOptions 
                { 
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                    WriteIndented = true
                };

                var jsonPayload = System.Text.Json.JsonSerializer.Serialize(request, options);

                Console.WriteLine("=================================================");
                Console.WriteLine($"[UpdateUnbilledCharge] Calling POST api/UnbilledCharges/update");
                Console.WriteLine($"[UpdateUnbilledCharge] Payload:\n{jsonPayload}");
                Console.WriteLine("=================================================");

                var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");
                var response = await _http.PostAsync("api/UnbilledCharges/update", content);
                var responseBody = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"[UpdateUnbilledCharge] Response Status: {(int)response.StatusCode} ({response.StatusCode})");
                Console.WriteLine($"[UpdateUnbilledCharge] Response Body: {responseBody}");
                Console.WriteLine("=================================================");

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                // Fallback to PUT if POST is not allowed
                if (response.StatusCode == System.Net.HttpStatusCode.MethodNotAllowed || 
                    response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    var putContent = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");
                    var putResponse = await _http.PutAsync("api/UnbilledCharges/update", putContent);
                    var putResponseBody = await putResponse.Content.ReadAsStringAsync();
                    Console.WriteLine($"[UpdateUnbilledCharge] PUT Response Status: {(int)putResponse.StatusCode} ({putResponse.StatusCode})");
                    Console.WriteLine($"[UpdateUnbilledCharge] PUT Response Body: {putResponseBody}");
                    if (putResponse.IsSuccessStatusCode) return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UpdateUnbilledCharge] Error updating unbilled charge: {ex.Message}");
                return false;
            }
        }

        public async Task<DiscardUnbilledResult?> DiscardUnbilledChargesAsync(List<string> unbilledIds)
        {
            try
            {
                if (unbilledIds == null || unbilledIds.Count == 0)
                {
                    Console.WriteLine("[DiscardUnbilledCharges] No unbilled IDs provided.");
                    return new DiscardUnbilledResult();
                }

                var request = new DiscardUnbilledChargesRequest
                {
                    unbilledids = unbilledIds.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList()
                };

                if (request.unbilledids.Count == 0)
                {
                    return new DiscardUnbilledResult();
                }

                var jsonPayload = System.Text.Json.JsonSerializer.Serialize(request);

                Console.WriteLine("=================================================");
                Console.WriteLine($"[DiscardUnbilledCharges] Calling POST api/UnbilledCharges/discard");
                Console.WriteLine($"[DiscardUnbilledCharges] Payload: {jsonPayload}");
                Console.WriteLine("=================================================");

                var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");
                var response = await _http.PostAsync("api/UnbilledCharges/discard", content);
                var responseBody = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"[DiscardUnbilledCharges] Response Status: {(int)response.StatusCode} ({response.StatusCode})");
                Console.WriteLine($"[DiscardUnbilledCharges] Response Body: {responseBody}");
                Console.WriteLine("=================================================");

                if (response.IsSuccessStatusCode && !string.IsNullOrWhiteSpace(responseBody))
                {
                    var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                    // 1. Try parsing wrapper { message: "...", result: { ... } }
                    try
                    {
                        var wrapped = System.Text.Json.JsonSerializer.Deserialize<DiscardUnbilledResponse>(responseBody, options);
                        if (wrapped?.result != null && (wrapped.result.requested > 0 || wrapped.result.discarded > 0 || (wrapped.result.discarded_ids != null && wrapped.result.discarded_ids.Any()) || (wrapped.result.skipped != null && wrapped.result.skipped.Any())))
                        {
                            return wrapped.result;
                        }

                        // Also try Newtonsoft if System.Text.Json was empty
                        var nWrapped = Newtonsoft.Json.JsonConvert.DeserializeObject<DiscardUnbilledResponse>(responseBody);
                        if (nWrapped?.result != null && (nWrapped.result.requested > 0 || nWrapped.result.discarded > 0 || (nWrapped.result.discarded_ids != null && nWrapped.result.discarded_ids.Any())))
                        {
                            return nWrapped.result;
                        }
                    }
                    catch { }

                    // 2. Try parsing direct DiscardUnbilledResult { requested: 1, discarded: 1, ... }
                    try
                    {
                        var direct = System.Text.Json.JsonSerializer.Deserialize<DiscardUnbilledResult>(responseBody, options);
                        if (direct != null && (direct.requested > 0 || direct.discarded > 0 || (direct.discarded_ids != null && direct.discarded_ids.Any())))
                        {
                            return direct;
                        }
                    }
                    catch { }

                    // 3. Fallback: If 200 OK and response contains "Discarded", construct successful result
                    if (responseBody.Contains("Discarded", StringComparison.OrdinalIgnoreCase))
                    {
                        return new DiscardUnbilledResult
                        {
                            requested = request.unbilledids.Count,
                            discarded = request.unbilledids.Count,
                            discarded_ids = request.unbilledids
                        };
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DiscardUnbilledCharges] Error discarding unbilled charges: {ex.Message}");
                return null;
            }
        }

        public async Task<DiscardUnbilledResult?> DiscardUnbilledChargeAsync(string unbilledId)
        {
            if (string.IsNullOrWhiteSpace(unbilledId)) return null;
            return await DiscardUnbilledChargesAsync(new List<string> { unbilledId });
        }

        private List<UnbilledChargeSummary> ParseUnbilledChargeSummaryList(string? rawJson)
        {
            if (string.IsNullOrWhiteSpace(rawJson))
                return new List<UnbilledChargeSummary>();

            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(rawJson);
                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    return System.Text.Json.JsonSerializer.Deserialize<List<UnbilledChargeSummary>>(rawJson, options) ?? new();
                }
                else if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Object)
                {
                    if (doc.RootElement.TryGetProperty("value", out var valueProp) && valueProp.ValueKind == System.Text.Json.JsonValueKind.Array)
                    {
                        return System.Text.Json.JsonSerializer.Deserialize<List<UnbilledChargeSummary>>(valueProp.GetRawText(), options) ?? new();
                    }
                    if (doc.RootElement.TryGetProperty("data", out var dataProp) && dataProp.ValueKind == System.Text.Json.JsonValueKind.Array)
                    {
                        return System.Text.Json.JsonSerializer.Deserialize<List<UnbilledChargeSummary>>(dataProp.GetRawText(), options) ?? new();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing unbilled charges JSON: {ex.Message}");
            }
            return new List<UnbilledChargeSummary>();
        }

        public async Task<BillingSummaryCombinedResponse?> GetBillingSummaryCombinedAsync(int custId, string? opNo = null, string? ipNo = null)
        {
            try
            {
                var queryParams = new List<string> { $"custid={custId}" };
                if (!string.IsNullOrWhiteSpace(opNo))
                {
                    queryParams.Add($"op_no={Uri.EscapeDataString(opNo.Trim())}");
                }
                if (!string.IsNullOrWhiteSpace(ipNo))
                {
                    queryParams.Add($"ip_no={Uri.EscapeDataString(ipNo.Trim())}");
                }

                string url = $"api/BillingSummary/combined?{string.Join("&", queryParams)}";
                Console.WriteLine($"[HmsBillingService] Calling: {url}");

                var client = GetClient();
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var rawJson = await response.Content.ReadAsStringAsync();
                    return System.Text.Json.JsonSerializer.Deserialize<BillingSummaryCombinedResponse>(rawJson, options);
                }
                else
                {
                    var err = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[HmsBillingService] GetBillingSummaryCombinedAsync failed ({response.StatusCode}): {err}");
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HmsBillingService] Error fetching combined billing summary: {ex.Message}");
                return null;
            }
        }
    }
}

