using LIMS_Backend.Model;
using SharedComponents.Rcl.Services;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Net.Http;

namespace LabCare.Services
{
    public class UserRightsService
    {
        private readonly HttpClient _http;
        private readonly TenantSessionState _session;

        public UserRightsService(HttpClient http, TenantSessionState session)
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

        public async Task<List<UserFormRightsModel.usermodules>> GetModulesAsync()
        {
            ConfigureHeaders();
            try { return await _http.GetFromJsonAsync<List<UserFormRightsModel.usermodules>>("api/userrights/modules") ?? new(); }
            catch { return new(); }
        }

        public async Task<List<UserFormRightsModel.usermodulerights>> GetModuleRightsAsync(int usercode)
        {
            ConfigureHeaders();
            try { return await _http.GetFromJsonAsync<List<UserFormRightsModel.usermodulerights>>($"api/userrights/modulerights?usercode={usercode}") ?? new(); }
            catch { return new(); }
        }

        public async Task<bool> SaveModuleRightsAsync(UserFormRightsModel.ModuleRightsUpsertRequest request)
        {
            ConfigureHeaders();
            
            var logBuilder = new System.Text.StringBuilder();
            logBuilder.AppendLine($"=== [SaveModuleRightsAsync] Called at {DateTime.Now} ===");
            logBuilder.AppendLine($"URL: {_http.BaseAddress}api/userrights/modulerights");
            logBuilder.AppendLine("Headers:");
            foreach (var h in _http.DefaultRequestHeaders)
            {
                logBuilder.AppendLine($"  {h.Key}: {string.Join(", ", h.Value)}");
            }
            
            string reqJson = "";
            try
            {
                reqJson = Newtonsoft.Json.JsonConvert.SerializeObject(request);
                logBuilder.AppendLine($"Request JSON: {reqJson}");
            }
            catch (Exception ex)
            {
                logBuilder.AppendLine($"Request serialization failed: {ex.Message}");
            }

            try
            {
                var response = await _http.PostAsJsonAsync("api/userrights/modulerights", request);
                logBuilder.AppendLine($"Response Status: {response.StatusCode}");
                
                var responseBody = await response.Content.ReadAsStringAsync();
                logBuilder.AppendLine($"Response Body: {responseBody}");
                
                System.IO.File.AppendAllText("api_logs.txt", logBuilder.ToString() + Environment.NewLine);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                logBuilder.AppendLine($"Exception: {ex}");
                System.IO.File.AppendAllText("api_logs.txt", logBuilder.ToString() + Environment.NewLine);
                return false;
            }
        }

        public async Task<bool> DeleteModuleRightsAsync(int usercode)
        {
            ConfigureHeaders();
            try
            {
                // API does not support HTTP DELETE (returns 405), use POST instead
                var response = await _http.PostAsync($"api/userrights/modulerightsdelete?usercode={usercode}", null);
                
                var logBuilder = new System.Text.StringBuilder();
                logBuilder.AppendLine($"=== [DeleteModuleRightsAsync] Called at {DateTime.Now} ===");
                logBuilder.AppendLine($"URL: {_http.BaseAddress}api/userrights/modulerightsdelete?usercode={usercode}");
                logBuilder.AppendLine($"Response Status: {response.StatusCode}");
                var responseBody = await response.Content.ReadAsStringAsync();
                logBuilder.AppendLine($"Response Body: {responseBody}");
                System.IO.File.AppendAllText("api_logs.txt", logBuilder.ToString() + Environment.NewLine);
                
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText("api_logs.txt", $"[DeleteModuleRightsAsync] Exception: {ex}{Environment.NewLine}");
                return false;
            }
        }
    }
}
