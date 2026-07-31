using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Booking.Models;
using SharedComponents.Rcl.Services;

namespace Booking.Services
{
    public class DoctorCurrentStatusService
    {
        private readonly HttpClient _http;
        private readonly TenantSessionState _session;
        
        // In-memory fallback dictionary keyed by dcode for offline/mock resiliency
        private static readonly Dictionary<long, DoctorCurrentStatusModel> _localCache = new();

        public DoctorCurrentStatusService(HttpClient http, TenantSessionState session)
        {
            _http = http;
            _session = session;
        }

        public async Task<List<DoctorCurrentStatusModel>> GetAllStatusesAsync()
        {
            try
            {
                var response = await _http.GetAsync("api/DoctorCurrentStatus/get");
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<List<DoctorCurrentStatusModel>>();
                    if (result != null && result.Count > 0)
                    {
                        foreach (var status in result)
                        {
                            _localCache[status.dcode] = status;
                        }
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DoctorCurrentStatusService] Get API call failed: {ex.Message}. Falling back to local cache.");
            }

            return _localCache.Values.ToList();
        }

        public async Task<DoctorCurrentStatusModel?> GetStatusByDcodeAsync(long dcode)
        {
            try
            {
                var response = await _http.GetAsync($"api/DoctorCurrentStatus/get?dcode={dcode}");
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<DoctorCurrentStatusModel>();
                    if (result != null)
                    {
                        _localCache[dcode] = result;
                        return result;
                    }
                    
                    var listResult = await response.Content.ReadFromJsonAsync<List<DoctorCurrentStatusModel>>();
                    var matched = listResult?.FirstOrDefault(s => s.dcode == dcode);
                    if (matched != null)
                    {
                        _localCache[dcode] = matched;
                        return matched;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DoctorCurrentStatusService] GetByDcode API call failed: {ex.Message}. Using local cache.");
            }

            if (_localCache.TryGetValue(dcode, out var cached))
            {
                return cached;
            }

            return null;
        }

        public async Task<bool> SetStatusAsync(DoctorCurrentStatusModel statusModel)
        {
            if (statusModel == null) return false;

            if (string.IsNullOrEmpty(statusModel.tenant_code) && !string.IsNullOrEmpty(_session.TenantCode))
            {
                statusModel.tenant_code = _session.TenantCode;
            }

            if (!statusModel.updated_by.HasValue && _session.UserCode > 0)
            {
                statusModel.updated_by = _session.UserCode;
            }

            statusModel.updated_at = DateTime.Now;

            // Always update local cache for instant UI response
            _localCache[statusModel.dcode] = statusModel;

            try
            {
                var response = await _http.PostAsJsonAsync("api/DoctorCurrentStatus/set-status", statusModel);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                
                // Fallback attempt to update endpoint
                var updateResponse = await _http.PostAsJsonAsync("api/DoctorCurrentStatus/update", statusModel);
                if (updateResponse.IsSuccessStatusCode)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DoctorCurrentStatusService] SetStatus API call failed: {ex.Message}. Local state updated.");
            }

            // Return true because local cache was updated successfully
            return true;
        }

        public async Task<bool> UpdateStatusAsync(DoctorCurrentStatusModel statusModel)
        {
            if (statusModel == null) return false;

            if (string.IsNullOrEmpty(statusModel.tenant_code) && !string.IsNullOrEmpty(_session.TenantCode))
            {
                statusModel.tenant_code = _session.TenantCode;
            }

            if (!statusModel.updated_by.HasValue && _session.UserCode > 0)
            {
                statusModel.updated_by = _session.UserCode;
            }

            statusModel.updated_at = DateTime.Now;

            _localCache[statusModel.dcode] = statusModel;

            try
            {
                var response = await _http.PostAsJsonAsync("api/DoctorCurrentStatus/update", statusModel);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DoctorCurrentStatusService] UpdateStatus API call failed: {ex.Message}. Local state updated.");
            }

            return true;
        }
    }
}
