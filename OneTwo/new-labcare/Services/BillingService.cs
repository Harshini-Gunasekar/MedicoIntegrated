using LabCare.Models;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Net.Http;

namespace LabCare.Services
{
    /// <summary>
    /// Service for handling all billing-related API interactions in LabCare Elite.
    /// </summary>
    public class BillingService
    {
        private readonly HttpClient _http;
        private readonly TenantSessionState _session;

        public BillingService(HttpClient http, TenantSessionState session)
        {
            _http = http;
            _session = session;
        }

        private void ConfigureHeaders()
        {
            if (!string.IsNullOrEmpty(_session.AuthToken))
            {
                _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.AuthToken);
            }
            if (!string.IsNullOrEmpty(_session.TenantCode))
            {
                if (_http.DefaultRequestHeaders.Contains("tenant_code")) _http.DefaultRequestHeaders.Remove("tenant_code");
                _http.DefaultRequestHeaders.Add("tenant_code", _session.TenantCode);
            }
        }

        // ════════════════════════════════════════════════════════════
        //  Core Transaction APIs (The Bill Lifecycle)
        // ════════════════════════════════════════════════════════════

        /// <summary>
        /// Unified save for both CREATE and UPDATE.
        /// <list type="bullet">
        ///   <item><c>dto.requestguid = null</c>  → creates a new bill</item>
        ///   <item><c>dto.requestguid = &lt;guid&gt;</c> → updates the existing bill</item>
        /// </list>
        /// Both paths call <c>POST billing/savebill</c>.
        /// </summary>
        public async Task<BillResponseDto?> SaveAsync(CreateBillDto dto)
        {
            ConfigureHeaders();
            var response = await _http.PostAsJsonAsync("billing/savebill", dto);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new Exception($"billing/savebill failed ({(int)response.StatusCode}): {body}");
            }
            var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return await response.Content.ReadFromJsonAsync<BillResponseDto>(options);
        }

        /// <inheritdoc cref="SaveAsync"/>
        [Obsolete("Use SaveAsync(CreateBillDto) instead — billing/savebill handles both create and update.")]
        public Task<BillResponseDto?> CreateAsync(CreateBillDto dto) => SaveAsync(dto);

        [Obsolete("Use SaveAsync(CreateBillDto with requestguid set) instead.")]
        public async Task<bool> UpdateAsync(UpdateBillDto dto)
        {
            // Bridge: map UpdateBillDto → CreateBillDto and call the unified endpoint
            var bridge = new CreateBillDto
            {
                requestguid        = dto.requestguid,
                dcode              = dto.dcode,
                ftcode             = dto.ftcode,
                pmcode             = dto.pmcode,
                discountper        = dto.discountper,
                discountamount     = dto.discountamount,
                specialdiscount    = dto.specialdiscount,
                paidamount         = dto.paidamount,
                iscashbill         = dto.iscashbill,
                iscreditbill       = dto.iscreditbill,
                billtype           = dto.billtype,
                concessionreason   = dto.concessionreason,
                card_refno         = dto.card_refno,
                bank_app           = dto.bank_app,
                seconddcode        = dto.seconddcode,
                seconddoctorname   = dto.seconddoctorname,
                usercode           = dto.usercode,
                computercode       = dto.computercode,
                doc1_dmid          = dto.doc1_dmid,
                doc1_dmvalue       = dto.doc1_dmvalue,
                doc2_dmid          = dto.doc2_dmid,
                doc2_dmvalue       = dto.doc2_dmvalue,
                doc1_path          = dto.doc1_path,
                doc2_path          = dto.doc2_path,
                testitems          = dto.testitems,
            };
            var result = await SaveAsync(bridge);
            return result != null;
        }

        public async Task<BillResponseDto?> GetByGuidAsync(Guid requestguid)
        {
            ConfigureHeaders();
            var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var response = await _http.GetAsync($"Billing/get?requestguid={requestguid}");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<BillResponseDto>(options);
        }

        public async Task<bool> DeleteAsync(Guid requestguid)
        {
            ConfigureHeaders();
            var response = await _http.DeleteAsync($"Billing/delete?requestguid={requestguid}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> CancelAsync(Guid requestguid)
        {
            ConfigureHeaders();
            var response = await _http.GetAsync($"Billing/cancel?requestguid={requestguid}");
            return response.IsSuccessStatusCode;
        }

        // ════════════════════════════════════════════════════════════
        //  Search & Filtering (High-Performance Reports)
        // ════════════════════════════════════════════════════════════

        public async Task<List<BillResponseDto>> GetAllAsync(BillFilterDto? filter = null)
        {
            ConfigureHeaders();
            HttpResponseMessage response;
            if (filter == null)
            {
                var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
                response = await _http.PostAsync("Billing/getall", content);
            }
            else
            {
                response = await _http.PostAsJsonAsync("Billing/getall", filter);
            }

            if (!response.IsSuccessStatusCode) return new();
            
            var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var paginatedResult = await response.Content.ReadFromJsonAsync<PaginatedBillingResponse>(options);
            return paginatedResult?.data ?? new();
        }

        public async Task<List<BillResponseDto>> SearchAsync(string keyword)
        {
            ConfigureHeaders();
            try { return await _http.GetFromJsonAsync<List<BillResponseDto>>($"Billing/search?keyword={Uri.EscapeDataString(keyword)}") ?? new(); }
            catch { return new(); }
        }

        public async Task<bool> CloseCounterAsync(CloseCounterDto dto)
        {
            ConfigureHeaders();
            var response = await _http.PostAsJsonAsync("Billing/closecounter", dto);
            return response.IsSuccessStatusCode;
        }

        // ════════════════════════════════════════════════════════════
        //  Payment & Audit APIs
        // ════════════════════════════════════════════════════════════

        public async Task<bool> CollectBalanceAsync(CollectBalanceDto dto)
        {
            ConfigureHeaders();
            var response = await _http.PostAsJsonAsync("Billing/collectbalance", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<List<PaymentHistoryDto>> GetPaymentHistoryAsync(Guid requestguid, int? usercode = null)
        {
            ConfigureHeaders();
            var url = $"Billing/paymenthistory?requestguid={requestguid}";
            if (usercode.HasValue) url += $"&usercode={usercode}";
            try { return await _http.GetFromJsonAsync<List<PaymentHistoryDto>>(url) ?? new(); }
            catch { return new(); }
        }

        // ════════════════════════════════════════════════════════════
        //  Catalog & Master Data
        // ════════════════════════════════════════════════════════════

        public async Task<List<test_master>> GetTestsAsync(string? keyword = null)
        {
            ConfigureHeaders();
            var url = "TestMaster/getall";
            if (!string.IsNullOrEmpty(keyword)) url += $"?keyword={Uri.EscapeDataString(keyword)}";
            try { return await _http.GetFromJsonAsync<List<test_master>>(url) ?? new(); }
            catch { return new(); }
        }

        public async Task<List<PayModeModel>> GetPayModesAsync()
        {
            ConfigureHeaders();
            try { return await _http.GetFromJsonAsync<List<PayModeModel>>("PayMode/getall") ?? new(); }
            catch { return new(); }
        }

        public async Task<List<BranchModel>> GetBranchesAsync()
        {
            ConfigureHeaders();
            try { return await _http.GetFromJsonAsync<List<BranchModel>>("Branch/get") ?? new(); }
            catch { return new(); }
        }

        public async Task<List<FeetypeModel>> GetFeeTypesAsync()
        {
            ConfigureHeaders();
            try { return await _http.GetFromJsonAsync<List<FeetypeModel>>("Feetype/get") ?? new(); }
            catch { return new(); }
        }

        public async Task<List<CounterModel>> GetCountersAsync(int? bhcode = null)
        {
            ConfigureHeaders();
            var url = "Counter/get";
            if (bhcode.HasValue) url += $"?bhcode={bhcode}";
            try { return await _http.GetFromJsonAsync<List<CounterModel>>(url) ?? new(); }
            catch { return new(); }
        }

        public async Task<TenantResponseDto?> GetTenantInfoAsync()
        {
            ConfigureHeaders();
            try { return await _http.GetFromJsonAsync<TenantResponseDto>("Billing/gettenant"); }
            catch { return null; }
        }

        /// <summary>
        /// Fetches bill data (lab_request_master + lab_request_details) for PDF printing.
        /// Calls GET billing/bill?requestguid={guid}
        /// </summary>
        public async Task<BillPrintDataDto?> GetBillForPrintAsync(Guid requestguid)
        {
            ConfigureHeaders();
            try
            {
                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var response = await _http.GetAsync($"billing/bill?requestguid={requestguid}");
                if (!response.IsSuccessStatusCode) return null;
                return await response.Content.ReadFromJsonAsync<BillPrintDataDto>(options);
            }
            catch { return null; }
        }
    }
}
