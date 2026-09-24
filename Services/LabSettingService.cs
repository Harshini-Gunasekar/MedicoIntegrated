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
        public static bool? CriticalValueIndicationCache { get; set; }
        public static bool? ShowPhysicalBillCache { get; set; }
        public static bool? PaymentRequiredOnlineRegCache { get; set; }
        public static bool? EnableComboRegistrationCache { get; set; }
        public static bool? AutoRouteBillingAfterRegisterCache { get; set; }

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
                
                if (!setting.show_all_customers.HasValue) setting.show_all_customers = true;
                if (!setting.is_slot_required.HasValue) setting.is_slot_required = true;
                if (!setting.op_age_wise_split.HasValue) setting.op_age_wise_split = false;
                if (!setting.critical_value_indication.HasValue) setting.critical_value_indication = false;
                if (!setting.show_physical_bill.HasValue) setting.show_physical_bill = true;
                if (!setting.payment_required_online_reg.HasValue) setting.payment_required_online_reg = false;
                if (!setting.enable_combo_registration.HasValue) setting.enable_combo_registration = false;
                if (!setting.auto_route_billing_after_register.HasValue) setting.auto_route_billing_after_register = false;

                return setting;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching LabSetting: {ex.Message}");
                return new LabSettingModel
                {
                    show_all_customers = true,
                    is_slot_required = true,
                    op_age_wise_split = false,
                    critical_value_indication = false,
                    show_physical_bill = true,
                    payment_required_online_reg = false,
                    enable_combo_registration = false,
                    auto_route_billing_after_register = false
                };
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
