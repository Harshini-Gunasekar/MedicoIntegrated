using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Booking.Models;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using SharedComponents.Rcl.Services;

namespace Booking.Services
{
    public class SlotService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        private readonly TenantSessionState? _tenantState;
        private readonly string _dbConn;

        public SlotService(HttpClient http, IConfiguration config, TenantSessionState? tenantState = null)
        {
            _http = http;
            _config = config;
            _tenantState = tenantState;
            _dbConn = _config.GetConnectionString("conn")
                      ?? "Host=103.108.220.19;Port=5432;Database=medico;Username=postgres;Password=wafewin;";
        }

        public async Task<List<DoctorAppointmentSlotMasterModel>> GetSlotsAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<DoctorAppointmentSlotMasterModel>>("api/DoctorAppointmentSlot/master/get");
                return response ?? new List<DoctorAppointmentSlotMasterModel>();
            }
            catch
            {
                return new List<DoctorAppointmentSlotMasterModel>();
            }
        }

        public async Task<List<DoctorAppointmentSlotMasterModel>> GetSlotsByDoctorAsync(int dcode)
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<DoctorAppointmentSlotMasterModel>>($"api/DoctorAppointmentSlot/master/get-by-doctor?dcode={dcode}");
                return response ?? new List<DoctorAppointmentSlotMasterModel>();
            }
            catch
            {
                return new List<DoctorAppointmentSlotMasterModel>();
            }
        }

        public async Task<SlotInsertResponse> InsertSlotAsync(object slots)
        {
            var response = await _http.PostAsJsonAsync("api/DoctorAppointmentSlot/master/insert", slots);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<SlotInsertResponse>() ?? new SlotInsertResponse();
            }
            return new SlotInsertResponse { failed = new List<object> { "API Error" } };
        }

        public async Task<bool> UpdateSlotAsync(object slots)
        {
            // 1. Attempt backend API first
            try
            {
                var response = await _http.PostAsJsonAsync("api/DoctorAppointmentSlot/master/update", slots);
                if (response.IsSuccessStatusCode)
                {
                    var apiResult = await response.Content.ReadFromJsonAsync<SlotUpdateResponse>();
                    if (apiResult != null && apiResult.updated != null && apiResult.updated.Any() && (apiResult.failed == null || !apiResult.failed.Any()))
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SlotService] API update error: {ex.Message}");
            }

            // 2. Direct database update fallback to ensure changes are reliably applied
            try
            {
                var json = JsonSerializer.Serialize(slots);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (root.ValueKind != JsonValueKind.Array)
                    return false;

                string tenant = !string.IsNullOrWhiteSpace(_tenantState?.TenantCode) ? _tenantState.TenantCode : "0012";
                if (_http.DefaultRequestHeaders.TryGetValues("tenant_code", out var hVals) && hVals.Any())
                {
                    tenant = hVals.First();
                }

                using IDbConnection db = new NpgsqlConnection(_dbConn);
                if (db.State != ConnectionState.Open)
                    db.Open();

                foreach (var el in root.EnumerateArray())
                {
                    if (!el.TryGetProperty("slot_master_id", out var midProp)) continue;
                    var midStr = midProp.GetString();
                    if (string.IsNullOrEmpty(midStr) || !Guid.TryParse(midStr, out var masterId)) continue;

                    string? typeofslot = el.TryGetProperty("typeofslot", out var ts) ? ts.GetString() : null;
                    int max_patients = el.TryGetProperty("max_patients", out var mp) && mp.TryGetInt32(out var mpVal) ? mpVal : 10;
                    int max_walkin = el.TryGetProperty("max_walkin", out var mw) && mw.TryGetInt32(out var mwVal) ? mwVal : 5;
                    int max_online = el.TryGetProperty("max_online", out var mo) && mo.TryGetInt32(out var moVal) ? moVal : 5;
                    int avgtime = el.TryGetProperty("avgtime", out var at) && at.TryGetInt32(out var atVal) ? atVal : 6;

                    string? startTimeStr = null;
                    if (el.TryGetProperty("slot_start_time", out var st) && TimeSpan.TryParse(st.GetString(), out var stVal))
                        startTimeStr = stVal.ToString(@"hh\:mm\:ss");

                    string? endTimeStr = null;
                    if (el.TryGetProperty("slot_end_time", out var et) && TimeSpan.TryParse(et.GetString(), out var etVal))
                        endTimeStr = etVal.ToString(@"hh\:mm\:ss");

                    string? slotDateStr = null;
                    if (el.TryGetProperty("slot_date", out var sd) && DateTime.TryParse(sd.GetString(), out var sdVal))
                        slotDateStr = sdVal.ToString("yyyy-MM-dd");

                    string masterSql = @"
                    UPDATE doctor_appointment_slot_master
                    SET
                        slot_start_time = COALESCE(CAST(@slot_start_time AS time), slot_start_time),
                        slot_end_time   = COALESCE(CAST(@slot_end_time AS time), slot_end_time),
                        typeofslot      = COALESCE(@typeofslot, typeofslot),
                        max_patients    = @max_patients,
                        max_walkin      = @max_walkin,
                        max_online      = @max_online,
                        avgtime         = @avgtime,
                        slot_date       = COALESCE(CAST(@slot_date AS date), slot_date),
                        updated_at      = now()
                    WHERE slot_master_id = @slot_master_id
                    AND   tenant_code    = @tenant_code";

                    await db.ExecuteAsync(masterSql, new
                    {
                        slot_start_time = startTimeStr,
                        slot_end_time = endTimeStr,
                        typeofslot,
                        max_patients,
                        max_walkin,
                        max_online,
                        avgtime,
                        slot_date = slotDateStr,
                        slot_master_id = masterId,
                        tenant_code = tenant
                    });

                    string syncDetailSql = @"
                    UPDATE doctor_appointment_slot_details
                    SET
                        slot_start_time = COALESCE(CAST(@slot_start_time AS time), slot_start_time),
                        slot_end_time   = COALESCE(CAST(@slot_end_time AS time), slot_end_time),
                        max_patients    = @max_patients,
                        max_walkin      = @max_walkin,
                        max_online      = @max_online,
                        typeofslot      = COALESCE(@typeofslot, typeofslot),
                        avgtime         = @avgtime,
                        updated_at      = now()
                    WHERE slot_master_id = @slot_master_id
                    AND   tenant_code    = @tenant_code
                    AND   isdeleted      = false";

                    await db.ExecuteAsync(syncDetailSql, new
                    {
                        slot_start_time = startTimeStr,
                        slot_end_time = endTimeStr,
                        max_patients,
                        max_walkin,
                        max_online,
                        typeofslot,
                        avgtime,
                        slot_master_id = masterId,
                        tenant_code = tenant
                    });
                }

                return true;
            }
            catch (Exception dbEx)
            {
                Console.WriteLine($"[SlotService] Direct DB update error: {dbEx.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteSlotAsync(Guid slot_master_id)
        {
            var response = await _http.GetAsync($"api/DoctorAppointmentSlot/master/delete?slot_master_id={slot_master_id}");
            return response.IsSuccessStatusCode;
        }
    }
}
