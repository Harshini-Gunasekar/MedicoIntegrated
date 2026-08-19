using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Booking.Models;

namespace Booking.Services
{
    public class LabSettingService
    {
        private readonly HttpClient _http;
        public static bool? ShowAllCustomersCache { get; set; }

        public LabSettingService(HttpClient http)
        {
            _http = http;
        }

        public async Task<LabSettingModel?> GetLabSettingAsync(int? bhCode = null)
        {
            try
            {
                var url = "api/LabSetting/get";
                if (bhCode.HasValue && bhCode.Value > 0)
                {
                    url += $"?bh_code={bhCode.Value}";
                }
                var list = await _http.GetFromJsonAsync<List<LabSettingModel>>(url);
                var setting = list?.FirstOrDefault() ?? new LabSettingModel();
                if (ShowAllCustomersCache.HasValue)
                {
                    setting.show_all_customers = ShowAllCustomersCache.Value;
                }
                else
                {
                    setting.show_all_customers = setting.show_all_customers ?? true;
                }
                return setting;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching LabSetting: {ex.Message}");
                var fallback = new LabSettingModel();
                if (ShowAllCustomersCache.HasValue)
                {
                    fallback.show_all_customers = ShowAllCustomersCache.Value;
                }
                return fallback;
            }
        }

        public async Task<bool> SaveLabSettingAsync(MultipartFormDataContent content, bool isUpdate = true)
        {
            try
            {
                var url = isUpdate ? "api/LabSetting/update" : "api/LabSetting/insert";
                var response = await _http.PostAsync(url, content);
                if (!response.IsSuccessStatusCode)
                {
                    var errStr = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[LabSettingService] Multipart Post ({response.StatusCode}): {errStr}");
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving LabSetting: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> InsertLabSettingAsync(LabSettingModel model)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/LabSetting/insert", model);
                if (!response.IsSuccessStatusCode)
                {
                    var errStr = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[LabSettingService] JSON Insert ({response.StatusCode}): {errStr}");
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting LabSetting: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateLabSettingAsync(LabSettingModel model)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/LabSetting/update", model);
                if (!response.IsSuccessStatusCode)
                {
                    var errStr = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[LabSettingService] JSON Update ({response.StatusCode}): {errStr}");
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating LabSetting: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SoftDeleteLabSettingAsync(Guid lsid)
        {
            try
            {
                var response = await _http.PostAsync($"api/LabSetting/delete?lsid={lsid}", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting LabSetting: {ex.Message}");
                return false;
            }
        }

        public async Task<LabSettingModel?> GetLabSettingsAsync()
        {
            return await GetLabSettingAsync();
        }
    }
}
