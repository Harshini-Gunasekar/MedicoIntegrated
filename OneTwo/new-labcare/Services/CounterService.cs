using LabCare.Models;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Net.Http;

namespace LabCare.Services
{
    public class CounterService
    {
        private readonly HttpClient _http;
        private readonly TenantSessionState _session;

        public CounterService(HttpClient http, TenantSessionState session)
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

        // --- Counter Master ---

        public async Task<List<CounterModel>> GetCountersAsync()
        {
            ConfigureHeaders();
            try { return await _http.GetFromJsonAsync<List<CounterModel>>("Counter/get") ?? new(); }
            catch { return new(); }
        }

        public async Task<CounterModel?> GetCounterByCodeAsync(long cntcode)
        {
            ConfigureHeaders();
            try { return await _http.GetFromJsonAsync<CounterModel>($"Counter/getbycode?cntcode={cntcode}"); }
            catch { return null; }
        }

        public async Task<bool> InsertCounterAsync(CounterModel counter)
        {
            ConfigureHeaders();
            var response = await _http.PostAsJsonAsync("Counter/insert", counter);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateCounterAsync(CounterModel counter)
        {
            ConfigureHeaders();
            var response = await _http.PostAsJsonAsync("Counter/update", counter);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> SoftDeleteCounterAsync(long cntcode)
        {
            ConfigureHeaders();
            var response = await _http.PostAsJsonAsync($"Counter/softdelete?cntcode={cntcode}", new { });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteCounterAsync(long cntcode)
        {
            ConfigureHeaders();
            var response = await _http.DeleteAsync($"Counter/delete?cntcode={cntcode}");
            return response.IsSuccessStatusCode;
        }

        // --- Counter Timing ---

        public async Task<List<CounterTimingModel>> GetCounterTimingsAsync(long cntcode)
        {
            ConfigureHeaders();
            try { return await _http.GetFromJsonAsync<List<CounterTimingModel>>($"counter/timing/get?cntcode={cntcode}") ?? new(); }
            catch { return new(); }
        }

        public async Task<bool> InsertCounterTimingAsync(CounterTimingModel timing)
        {
            ConfigureHeaders();
            var response = await _http.PostAsJsonAsync("counter/timing/insert", timing);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateCounterTimingAsync(CounterTimingModel timing)
        {
            ConfigureHeaders();
            var response = await _http.PostAsJsonAsync("counter/timing/update", timing);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteCounterTimingAsync(Guid cnttid)
        {
            ConfigureHeaders();
            var response = await _http.DeleteAsync($"counter/timing/delete?cnttid={cnttid}");
            return response.IsSuccessStatusCode;
        }
    }
}
