using LabCare.Models;
using LIMS_Backend.Model;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Net.Http;
using SharedComponents.Rcl.Services;

namespace LabCare.Services
{
    public class GroupService
    {
        private readonly HttpClient _http;
        private readonly TenantSessionState _session;

        public GroupService(HttpClient http, TenantSessionState session)
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

        public async Task<List<GroupModel>> GetAllAsync()
        {
            ConfigureHeaders();
            try { return await _http.GetFromJsonAsync<List<GroupModel>>("GroupMaster/get") ?? new(); }
            catch { return new(); }
        }

        public async Task<bool> InsertAsync(GroupModel group)
        {
            ConfigureHeaders();
            group.tenant_code = _session.TenantCode;
            group.entereddate = DateTime.Now;
            group.ibsdate = DateTime.Now;
            var response = await _http.PostAsJsonAsync("GroupMaster/insert", group);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(GroupModel group)
        {
            ConfigureHeaders();
            group.tenant_code = _session.TenantCode;
            group.ibsdate = DateTime.Now;
            var response = await _http.PostAsJsonAsync("GroupMaster/update", group);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> SoftDeleteAsync(long gcode)
        {
            ConfigureHeaders();
            var response = await _http.DeleteAsync($"GroupMaster/softdelete?gcode={gcode}");
            if (!response.IsSuccessStatusCode)
            {
                var getResponse = await _http.GetAsync($"GroupMaster/softdelete?gcode={gcode}");
                return getResponse.IsSuccessStatusCode;
            }
            return response.IsSuccessStatusCode;
        }
    }
}
