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
                return list?.FirstOrDefault() ?? new LabSettingModel();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching LabSetting: {ex.Message}");
                return new LabSettingModel();
            }
        }

        public async Task<bool> SaveLabSettingAsync(MultipartFormDataContent content, bool isUpdate = true)
        {
            try
            {
                var url = isUpdate ? "api/LabSetting/update" : "api/LabSetting/insert";
                var response = await _http.PostAsync(url, content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving LabSetting: {ex.Message}");
                return false;
            }
        }
    }
}
