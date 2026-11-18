using LabCare.Models;
using LIMS_Backend.Model;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace LabCare.Services
{
    public class SampleCollectionService
    {
        private readonly HttpClient _http;
        private readonly TenantSessionState _session;

        public SampleCollectionService(HttpClient http, TenantSessionState session)
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

        public async Task<List<LoadSampleCollectionDto>> LoadSampleCollectionAsync(string fromDate, string toDate)
        {
            ConfigureHeaders();
            try
            {
                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var response = await _http.GetAsync($"labsamplecollection/loadsamplecollection?fromdate={fromDate}&todate={toDate}");
                if (!response.IsSuccessStatusCode) return new();
                return await response.Content.ReadFromJsonAsync<List<LoadSampleCollectionDto>>(options) ?? new();
            }
            catch
            {
                return new();
            }
        }

        public async Task<List<LoadSampleReceivedDto>> LoadSampleReceivedAsync(string fromDate, string toDate)
        {
            ConfigureHeaders();
            try
            {
                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var response = await _http.GetAsync($"labsamplecollection/loadsamplereceived?fromdate={fromDate}&todate={toDate}");
                if (!response.IsSuccessStatusCode) return new();
                return await response.Content.ReadFromJsonAsync<List<LoadSampleReceivedDto>>(options) ?? new();
            }
            catch
            {
                return new();
            }
        }

        public async Task<bool> SaveSampleCollectionAsync(SaveSampleCollectionRequest request)
        {
            ConfigureHeaders();
            var response = await _http.PostAsJsonAsync("labsamplecollection/savesamplecollection", request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> SaveSampleReceiveAsync(SaveSampleReceiveRequest request)
        {
            ConfigureHeaders();
            var response = await _http.PostAsJsonAsync("labsamplecollection/savesamplereceived", request);
            return response.IsSuccessStatusCode;
        }
    }
}
