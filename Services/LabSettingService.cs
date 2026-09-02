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
        public static bool? IsSlotRequiredCache { get; set; }
        public static bool? OpAgeWiseSplitCache { get; set; }
        public static bool? CriticalValueIndicationCache { get; set; }
        public static bool? ShowPhysicalBillCache { get; set; }

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
                else if (setting.show_all_customers.HasValue)
                {
                    ShowAllCustomersCache = setting.show_all_customers.Value;
                }
                else
                {
                    setting.show_all_customers = true;
                    ShowAllCustomersCache = true;
                }

                if (IsSlotRequiredCache.HasValue)
                {
                    setting.is_slot_required = IsSlotRequiredCache.Value;
                }
                else if (setting.is_slot_required.HasValue)
                {
                    IsSlotRequiredCache = setting.is_slot_required.Value;
                }
                else
                {
                    setting.is_slot_required = true;
                    IsSlotRequiredCache = true;
                }

                if (OpAgeWiseSplitCache.HasValue)
                {
                    setting.op_age_wise_split = OpAgeWiseSplitCache.Value;
                }
                else if (setting.op_age_wise_split.HasValue)
                {
                    OpAgeWiseSplitCache = setting.op_age_wise_split.Value;
                }
                else
                {
                    setting.op_age_wise_split = false;
                    OpAgeWiseSplitCache = false;
                }

                if (CriticalValueIndicationCache.HasValue)
                {
                    setting.critical_value_indication = CriticalValueIndicationCache.Value;
                }
                else if (setting.critical_value_indication.HasValue)
                {
                    CriticalValueIndicationCache = setting.critical_value_indication.Value;
                }
                else
                {
                    setting.critical_value_indication = false;
                    CriticalValueIndicationCache = false;
                }

                if (ShowPhysicalBillCache.HasValue)
                {
                    setting.show_physical_bill = ShowPhysicalBillCache.Value;
                }
                else if (setting.show_physical_bill.HasValue)
                {
                    ShowPhysicalBillCache = setting.show_physical_bill.Value;
                }
                else
                {
                    setting.show_physical_bill = true;
                    ShowPhysicalBillCache = true;
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
                if (IsSlotRequiredCache.HasValue)
                {
                    fallback.is_slot_required = IsSlotRequiredCache.Value;
                }
                if (OpAgeWiseSplitCache.HasValue)
                {
                    fallback.op_age_wise_split = OpAgeWiseSplitCache.Value;
                }
                if (CriticalValueIndicationCache.HasValue)
                {
                    fallback.critical_value_indication = CriticalValueIndicationCache.Value;
                }
                if (ShowPhysicalBillCache.HasValue)
                {
                    fallback.show_physical_bill = ShowPhysicalBillCache.Value;
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
                var errStr = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[LabSettingService] Multipart Post to '{url}' ({response.StatusCode}): {errStr}");
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
                if (model.show_all_customers.HasValue)
                {
                    ShowAllCustomersCache = model.show_all_customers.Value;
                }
                if (model.is_slot_required.HasValue)
                {
                    IsSlotRequiredCache = model.is_slot_required.Value;
                }
                if (model.op_age_wise_split.HasValue)
                {
                    OpAgeWiseSplitCache = model.op_age_wise_split.Value;
                }
                if (model.critical_value_indication.HasValue)
                {
                    CriticalValueIndicationCache = model.critical_value_indication.Value;
                }
                if (model.show_physical_bill.HasValue)
                {
                    ShowPhysicalBillCache = model.show_physical_bill.Value;
                }

                var jsonOptions = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                var jsonPayload = System.Text.Json.JsonSerializer.Serialize(model, jsonOptions);
                Console.WriteLine($"[LabSettingService] Insert Payload:\n{jsonPayload}");

                var response = await _http.PostAsJsonAsync("api/LabSetting/insert", model);
                var errStr = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[LabSettingService] JSON Insert Response ({response.StatusCode}): {errStr}");
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
                if (model.show_all_customers.HasValue)
                {
                    ShowAllCustomersCache = model.show_all_customers.Value;
                }
                if (model.is_slot_required.HasValue)
                {
                    IsSlotRequiredCache = model.is_slot_required.Value;
                }
                if (model.op_age_wise_split.HasValue)
                {
                    OpAgeWiseSplitCache = model.op_age_wise_split.Value;
                }
                if (model.critical_value_indication.HasValue)
                {
                    CriticalValueIndicationCache = model.critical_value_indication.Value;
                }
                if (model.show_physical_bill.HasValue)
                {
                    ShowPhysicalBillCache = model.show_physical_bill.Value;
                }

                var jsonOptions = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                var jsonPayload = System.Text.Json.JsonSerializer.Serialize(model, jsonOptions);
                Console.WriteLine($"[LabSettingService] Update Payload:\n{jsonPayload}");

                var response = await _http.PostAsJsonAsync("api/LabSetting/update", model);
                var errStr = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[LabSettingService] JSON Update Response ({response.StatusCode}): {errStr}");
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
