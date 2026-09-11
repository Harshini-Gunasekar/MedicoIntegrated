using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using medico_backend.Model;

namespace Booking.Services
{
    public class OtApiResponse
    {
        public string message { get; set; } = string.Empty;
        public bool IsSuccess => !string.IsNullOrWhiteSpace(message) && message.StartsWith("Success", StringComparison.OrdinalIgnoreCase);
        public string? GeneratedId => message.Contains("|Id:") ? message.Split("|Id:")[1].Trim() : null;
    }

    public class OtService
    {
        private readonly HttpClient _http;
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public OtService(HttpClient http)
        {
            _http = http;
        }

        private async Task<OtApiResponse> HandleResponseAsync(HttpResponseMessage response)
        {
            try
            {
                var content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content))
                {
                    return new OtApiResponse { message = response.IsSuccessStatusCode ? "Success" : "Failed with status " + response.StatusCode };
                }

                var parsed = JsonSerializer.Deserialize<OtApiResponse>(content, _jsonOptions);
                if (parsed != null && !string.IsNullOrWhiteSpace(parsed.message))
                {
                    return parsed;
                }

                return new OtApiResponse { message = response.IsSuccessStatusCode ? "Success" : content };
            }
            catch (Exception ex)
            {
                return new OtApiResponse { message = response.IsSuccessStatusCode ? "Success" : ex.Message };
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // 1. OT MASTER
        // ═══════════════════════════════════════════════════════════════
        public async Task<List<OtMasterModel>> GetOtMastersAsync(bool activeOnly = false)
        {
            try
            {
                var url = $"api/OT/master/list?activeOnly={activeOnly.ToString().ToLower()}";
                var res = await _http.GetFromJsonAsync<List<OtMasterModel>>(url, _jsonOptions);
                return res ?? new List<OtMasterModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OtService.GetOtMastersAsync] Error: {ex.Message}");
                return new List<OtMasterModel>();
            }
        }

        public async Task<OtMasterModel?> GetOtMasterByIdAsync(Guid otId)
        {
            try
            {
                return await _http.GetFromJsonAsync<OtMasterModel>($"api/OT/master/{otId}", _jsonOptions);
            }
            catch
            {
                return null;
            }
        }

        public async Task<OtApiResponse> AddOtMasterAsync(AddOtMasterRequest req)
        {
            try
            {
                var res = await _http.PostAsJsonAsync("api/OT/master/add", req);
                return await HandleResponseAsync(res);
            }
            catch (Exception ex)
            {
                return new OtApiResponse { message = $"Error: {ex.Message}" };
            }
        }

        public async Task<OtApiResponse> UpdateOtMasterAsync(UpdateOtMasterRequest req)
        {
            try
            {
                var res = await _http.PostAsJsonAsync("api/OT/master/update", req);
                return await HandleResponseAsync(res);
            }
            catch (Exception ex)
            {
                return new OtApiResponse { message = $"Error: {ex.Message}" };
            }
        }

        public async Task<OtApiResponse> DeleteOtMasterAsync(Guid otId)
        {
            try
            {
                var res = await _http.GetAsync($"api/OT/master/delete?ot_id={otId}");
                return await HandleResponseAsync(res);
            }
            catch (Exception ex)
            {
                return new OtApiResponse { message = $"Error: {ex.Message}" };
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // 2. SURGERY REQUEST
        // ═══════════════════════════════════════════════════════════════
        public async Task<List<OtSurgeryRequestListModel>> GetSurgeryRequestsAsync(string? status = null, decimal? custid = null)
        {
            try
            {
                var queryParams = new List<string>();
                if (!string.IsNullOrWhiteSpace(status)) queryParams.Add($"status={Uri.EscapeDataString(status)}");
                if (custid.HasValue && custid.Value > 0) queryParams.Add($"custid={custid.Value}");

                var url = "api/OT/surgery-request/list";
                if (queryParams.Count > 0) url += "?" + string.Join("&", queryParams);

                var res = await _http.GetFromJsonAsync<List<OtSurgeryRequestListModel>>(url, _jsonOptions);
                return res ?? new List<OtSurgeryRequestListModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OtService.GetSurgeryRequestsAsync] Error: {ex.Message}");
                return new List<OtSurgeryRequestListModel>();
            }
        }

        public async Task<OtSurgeryRequestModel?> GetSurgeryRequestByIdAsync(Guid requestId)
        {
            try
            {
                return await _http.GetFromJsonAsync<OtSurgeryRequestModel>($"api/OT/surgery-request/{requestId}", _jsonOptions);
            }
            catch
            {
                return null;
            }
        }

        public async Task<OtApiResponse> AddSurgeryRequestAsync(AddSurgeryRequestRequest req)
        {
            try
            {
                var res = await _http.PostAsJsonAsync("api/OT/surgery-request/add", req);
                return await HandleResponseAsync(res);
            }
            catch (Exception ex)
            {
                return new OtApiResponse { message = $"Error: {ex.Message}" };
            }
        }

        public async Task<OtApiResponse> UpdateSurgeryRequestAsync(UpdateSurgeryRequestRequest req)
        {
            try
            {
                var res = await _http.PostAsJsonAsync("api/OT/surgery-request/update", req);
                return await HandleResponseAsync(res);
            }
            catch (Exception ex)
            {
                return new OtApiResponse { message = $"Error: {ex.Message}" };
            }
        }

        public async Task<OtApiResponse> UpdateSurgeryStatusAsync(UpdateSurgeryStatusRequest req)
        {
            try
            {
                var res = await _http.PostAsJsonAsync("api/OT/surgery-request/status", req);
                return await HandleResponseAsync(res);
            }
            catch (Exception ex)
            {
                return new OtApiResponse { message = $"Error: {ex.Message}" };
            }
        }

        public async Task<OtApiResponse> CancelSurgeryRequestAsync(Guid requestId)
        {
            try
            {
                var res = await _http.GetAsync($"api/OT/surgery-request/cancel?request_id={requestId}");
                return await HandleResponseAsync(res);
            }
            catch (Exception ex)
            {
                return new OtApiResponse { message = $"Error: {ex.Message}" };
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // 3. OT SCHEDULE
        // ═══════════════════════════════════════════════════════════════
        public async Task<List<OtScheduleModel>> GetScheduleCalendarAsync(Guid? otId = null, string? fromDate = null, string? toDate = null)
        {
            try
            {
                var queryParams = new List<string>();
                if (otId.HasValue && otId.Value != Guid.Empty) queryParams.Add($"ot_id={otId.Value}");
                if (!string.IsNullOrWhiteSpace(fromDate)) queryParams.Add($"from_date={Uri.EscapeDataString(fromDate)}");
                if (!string.IsNullOrWhiteSpace(toDate)) queryParams.Add($"to_date={Uri.EscapeDataString(toDate)}");

                var url = "api/OT/schedule/calendar";
                if (queryParams.Count > 0) url += "?" + string.Join("&", queryParams);

                var res = await _http.GetFromJsonAsync<List<OtScheduleModel>>(url, _jsonOptions);
                return res ?? new List<OtScheduleModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OtService.GetScheduleCalendarAsync] Error: {ex.Message}");
                return new List<OtScheduleModel>();
            }
        }

        public async Task<OtScheduleModel?> GetScheduleByRequestAsync(Guid requestId)
        {
            try
            {
                return await _http.GetFromJsonAsync<OtScheduleModel>($"api/OT/schedule/by-request?request_id={requestId}", _jsonOptions);
            }
            catch
            {
                return null;
            }
        }

        public async Task<OtApiResponse> AddScheduleAsync(AddOtScheduleRequest req)
        {
            try
            {
                var res = await _http.PostAsJsonAsync("api/OT/schedule/add", req);
                return await HandleResponseAsync(res);
            }
            catch (Exception ex)
            {
                return new OtApiResponse { message = $"Error: {ex.Message}" };
            }
        }

        public async Task<OtApiResponse> UpdateScheduleAsync(UpdateOtScheduleRequest req)
        {
            try
            {
                var res = await _http.PostAsJsonAsync("api/OT/schedule/update", req);
                return await HandleResponseAsync(res);
            }
            catch (Exception ex)
            {
                return new OtApiResponse { message = $"Error: {ex.Message}" };
            }
        }

        public async Task<OtApiResponse> CancelScheduleAsync(Guid scheduleId)
        {
            try
            {
                var res = await _http.GetAsync($"api/OT/schedule/cancel?schedule_id={scheduleId}");
                return await HandleResponseAsync(res);
            }
            catch (Exception ex)
            {
                return new OtApiResponse { message = $"Error: {ex.Message}" };
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // 4. OT TEAM
        // ═══════════════════════════════════════════════════════════════
        public async Task<List<OtTeamMemberModel>> GetTeamByRequestAsync(Guid requestId)
        {
            try
            {
                var res = await _http.GetFromJsonAsync<List<OtTeamMemberModel>>($"api/OT/team/by-request?request_id={requestId}", _jsonOptions);
                return res ?? new List<OtTeamMemberModel>();
            }
            catch
            {
                return new List<OtTeamMemberModel>();
            }
        }

        public async Task<OtApiResponse> AddTeamMemberAsync(AddOtTeamMemberRequest req)
        {
            try
            {
                var res = await _http.PostAsJsonAsync("api/OT/team/add", req);
                return await HandleResponseAsync(res);
            }
            catch (Exception ex)
            {
                return new OtApiResponse { message = $"Error: {ex.Message}" };
            }
        }

        public async Task<OtApiResponse> RemoveTeamMemberAsync(Guid teamId)
        {
            try
            {
                var res = await _http.GetAsync($"api/OT/team/remove?team_id={teamId}");
                return await HandleResponseAsync(res);
            }
            catch (Exception ex)
            {
                return new OtApiResponse { message = $"Error: {ex.Message}" };
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // 5. PRE-OP ASSESSMENT
        // ═══════════════════════════════════════════════════════════════
        public async Task<OtPreOpAssessmentModel?> GetPreOpByRequestAsync(Guid requestId)
        {
            try
            {
                return await _http.GetFromJsonAsync<OtPreOpAssessmentModel>($"api/OT/preop/by-request?request_id={requestId}", _jsonOptions);
            }
            catch
            {
                return null;
            }
        }

        public async Task<OtApiResponse> AddPreOpAsync(AddPreOpAssessmentRequest req)
        {
            try
            {
                var res = await _http.PostAsJsonAsync("api/OT/preop/add", req);
                return await HandleResponseAsync(res);
            }
            catch (Exception ex)
            {
                return new OtApiResponse { message = $"Error: {ex.Message}" };
            }
        }

        public async Task<OtApiResponse> UpdatePreOpAsync(UpdatePreOpAssessmentRequest req)
        {
            try
            {
                var res = await _http.PostAsJsonAsync("api/OT/preop/update", req);
                return await HandleResponseAsync(res);
            }
            catch (Exception ex)
            {
                return new OtApiResponse { message = $"Error: {ex.Message}" };
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // 6. CONSENT
        // ═══════════════════════════════════════════════════════════════
        public async Task<List<OtConsentModel>> GetConsentsByRequestAsync(Guid requestId)
        {
            try
            {
                var res = await _http.GetFromJsonAsync<List<OtConsentModel>>($"api/OT/consent/by-request?request_id={requestId}", _jsonOptions);
                return res ?? new List<OtConsentModel>();
            }
            catch
            {
                return new List<OtConsentModel>();
            }
        }

        public async Task<OtApiResponse> AddConsentAsync(AddOtConsentRequest req)
        {
            try
            {
                var res = await _http.PostAsJsonAsync("api/OT/consent/add", req);
                return await HandleResponseAsync(res);
            }
            catch (Exception ex)
            {
                return new OtApiResponse { message = $"Error: {ex.Message}" };
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // 7. OT CHECKLIST (WHO Surgical Safety)
        // ═══════════════════════════════════════════════════════════════
        public async Task<OtChecklistModel?> GetChecklistByRequestAsync(Guid requestId)
        {
            try
            {
                return await _http.GetFromJsonAsync<OtChecklistModel>($"api/OT/checklist/by-request?request_id={requestId}", _jsonOptions);
            }
            catch
            {
                return null;
            }
        }

        public async Task<OtApiResponse> SaveChecklistAsync(SaveOtChecklistRequest req)
        {
            try
            {
                var res = await _http.PostAsJsonAsync("api/OT/checklist/save", req);
                return await HandleResponseAsync(res);
            }
            catch (Exception ex)
            {
                return new OtApiResponse { message = $"Error: {ex.Message}" };
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // 8. ANESTHESIA
        // ═══════════════════════════════════════════════════════════════
        public async Task<OtAnesthesiaModel?> GetAnesthesiaByRequestAsync(Guid requestId)
        {
            try
            {
                return await _http.GetFromJsonAsync<OtAnesthesiaModel>($"api/OT/anesthesia/by-request?request_id={requestId}", _jsonOptions);
            }
            catch
            {
                return null;
            }
        }

        public async Task<OtApiResponse> SaveAnesthesiaAsync(SaveOtAnesthesiaRequest req)
        {
            try
            {
                var res = await _http.PostAsJsonAsync("api/OT/anesthesia/save", req);
                return await HandleResponseAsync(res);
            }
            catch (Exception ex)
            {
                return new OtApiResponse { message = $"Error: {ex.Message}" };
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // 9. INTRA-OP / SURGERY RECORD
        // ═══════════════════════════════════════════════════════════════
        public async Task<OtIntraOpModel?> GetIntraOpByRequestAsync(Guid requestId)
        {
            try
            {
                return await _http.GetFromJsonAsync<OtIntraOpModel>($"api/OT/intraop/by-request?request_id={requestId}", _jsonOptions);
            }
            catch
            {
                return null;
            }
        }

        public async Task<OtApiResponse> SaveIntraOpAsync(SaveOtIntraOpRequest req)
        {
            try
            {
                var res = await _http.PostAsJsonAsync("api/OT/intraop/save", req);
                return await HandleResponseAsync(res);
            }
            catch (Exception ex)
            {
                return new OtApiResponse { message = $"Error: {ex.Message}" };
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // 10. CONSUMABLES
        // ═══════════════════════════════════════════════════════════════
        public async Task<List<OtConsumableModel>> GetConsumablesByRequestAsync(Guid requestId)
        {
            try
            {
                var res = await _http.GetFromJsonAsync<List<OtConsumableModel>>($"api/OT/consumable/by-request?request_id={requestId}", _jsonOptions);
                return res ?? new List<OtConsumableModel>();
            }
            catch
            {
                return new List<OtConsumableModel>();
            }
        }

        public async Task<OtApiResponse> AddConsumableAsync(AddOtConsumableRequest req)
        {
            try
            {
                var res = await _http.PostAsJsonAsync("api/OT/consumable/add", req);
                return await HandleResponseAsync(res);
            }
            catch (Exception ex)
            {
                return new OtApiResponse { message = $"Error: {ex.Message}" };
            }
        }

        public async Task<OtApiResponse> DeleteConsumableAsync(Guid consumableId)
        {
            try
            {
                var res = await _http.GetAsync($"api/OT/consumable/delete?consumable_id={consumableId}");
                return await HandleResponseAsync(res);
            }
            catch (Exception ex)
            {
                return new OtApiResponse { message = $"Error: {ex.Message}" };
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // 11. IMPLANTS
        // ═══════════════════════════════════════════════════════════════
        public async Task<List<OtImplantModel>> GetImplantsByRequestAsync(Guid requestId)
        {
            try
            {
                var res = await _http.GetFromJsonAsync<List<OtImplantModel>>($"api/OT/implant/by-request?request_id={requestId}", _jsonOptions);
                return res ?? new List<OtImplantModel>();
            }
            catch
            {
                return new List<OtImplantModel>();
            }
        }

        public async Task<OtApiResponse> AddImplantAsync(AddOtImplantRequest req)
        {
            try
            {
                var res = await _http.PostAsJsonAsync("api/OT/implant/add", req);
                return await HandleResponseAsync(res);
            }
            catch (Exception ex)
            {
                return new OtApiResponse { message = $"Error: {ex.Message}" };
            }
        }

        public async Task<OtApiResponse> DeleteImplantAsync(Guid implantId)
        {
            try
            {
                var res = await _http.GetAsync($"api/OT/implant/delete?implant_id={implantId}");
                return await HandleResponseAsync(res);
            }
            catch (Exception ex)
            {
                return new OtApiResponse { message = $"Error: {ex.Message}" };
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // 12. RECOVERY / PACU
        // ═══════════════════════════════════════════════════════════════
        public async Task<OtRecoveryModel?> GetRecoveryByRequestAsync(Guid requestId)
        {
            try
            {
                return await _http.GetFromJsonAsync<OtRecoveryModel>($"api/OT/recovery/by-request?request_id={requestId}", _jsonOptions);
            }
            catch
            {
                return null;
            }
        }

        public async Task<OtApiResponse> SaveRecoveryAsync(SaveOtRecoveryRequest req)
        {
            try
            {
                var res = await _http.PostAsJsonAsync("api/OT/recovery/save", req);
                return await HandleResponseAsync(res);
            }
            catch (Exception ex)
            {
                return new OtApiResponse { message = $"Error: {ex.Message}" };
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // 13. POST-OP ORDERS
        // ═══════════════════════════════════════════════════════════════
        public async Task<List<OtPostOpOrderModel>> GetPostOpOrdersByRequestAsync(Guid requestId)
        {
            try
            {
                var res = await _http.GetFromJsonAsync<List<OtPostOpOrderModel>>($"api/OT/postop-order/by-request?request_id={requestId}", _jsonOptions);
                return res ?? new List<OtPostOpOrderModel>();
            }
            catch
            {
                return new List<OtPostOpOrderModel>();
            }
        }

        public async Task<OtApiResponse> AddPostOpOrderAsync(AddOtPostOpOrderRequest req)
        {
            try
            {
                var res = await _http.PostAsJsonAsync("api/OT/postop-order/add", req);
                return await HandleResponseAsync(res);
            }
            catch (Exception ex)
            {
                return new OtApiResponse { message = $"Error: {ex.Message}" };
            }
        }

        public async Task<OtApiResponse> UpdatePostOpOrderStatusAsync(UpdateOtPostOpOrderStatusRequest req)
        {
            try
            {
                var res = await _http.PostAsJsonAsync("api/OT/postop-order/status", req);
                return await HandleResponseAsync(res);
            }
            catch (Exception ex)
            {
                return new OtApiResponse { message = $"Error: {ex.Message}" };
            }
        }

        public async Task<OtApiResponse> DeletePostOpOrderAsync(Guid orderId)
        {
            try
            {
                var res = await _http.GetAsync($"api/OT/postop-order/delete?order_id={orderId}");
                return await HandleResponseAsync(res);
            }
            catch (Exception ex)
            {
                return new OtApiResponse { message = $"Error: {ex.Message}" };
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // 14. OPERATION NOTE
        // ═══════════════════════════════════════════════════════════════
        public async Task<OtOperationNoteModel?> GetOperationNoteByRequestAsync(Guid requestId)
        {
            try
            {
                return await _http.GetFromJsonAsync<OtOperationNoteModel>($"api/OT/operation-note/by-request?request_id={requestId}", _jsonOptions);
            }
            catch
            {
                return null;
            }
        }

        public async Task<OtApiResponse> SaveOperationNoteAsync(SaveOtOperationNoteRequest req)
        {
            try
            {
                var res = await _http.PostAsJsonAsync("api/OT/operation-note/save", req);
                return await HandleResponseAsync(res);
            }
            catch (Exception ex)
            {
                return new OtApiResponse { message = $"Error: {ex.Message}" };
            }
        }

        public async Task<OtApiResponse> PostponeScheduleAsync(PostponeOtScheduleRequest req)
        {
            try
            {
                var res = await _http.PostAsJsonAsync("api/OT/schedule/postpone", req);
                return await HandleResponseAsync(res);
            }
            catch (Exception ex)
            {
                return new OtApiResponse { message = $"Error: {ex.Message}" };
            }
        }

        public async Task<List<OtMasterModel>> GetAvailabilityAsync(DateOnly? scheduledDate = null, TimeOnly? scheduledTime = null, TimeOnly? estimatedEndTime = null, string? specialty = null)
        {
            try
            {
                var queryParams = new List<string>();
                if (scheduledDate.HasValue) queryParams.Add($"scheduled_date={scheduledDate.Value:yyyy-MM-dd}");
                if (scheduledTime.HasValue) queryParams.Add($"scheduled_time={scheduledTime.Value:HH\\:mm}");
                if (estimatedEndTime.HasValue) queryParams.Add($"estimated_end_time={estimatedEndTime.Value:HH\\:mm}");
                if (!string.IsNullOrWhiteSpace(specialty)) queryParams.Add($"specialty={Uri.EscapeDataString(specialty)}");

                var url = "api/OT/availability";
                if (queryParams.Count > 0) url += "?" + string.Join("&", queryParams);

                var res = await _http.GetFromJsonAsync<List<OtMasterModel>>(url, _jsonOptions);
                return res ?? new List<OtMasterModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OtService.GetAvailabilityAsync] Error: {ex.Message}");
                return new List<OtMasterModel>();
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // 16. STERILIZATION
        // ═══════════════════════════════════════════════════════════════
        public async Task<List<OtSterilizationModel>> GetSterilizationsByRequestAsync(Guid requestId)
        {
            try
            {
                var res = await _http.GetFromJsonAsync<List<OtSterilizationModel>>($"api/OT/sterilization/by-request?request_id={requestId}", _jsonOptions);
                return res ?? new List<OtSterilizationModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OtService.GetSterilizationsByRequestAsync] Error: {ex.Message}");
                return new List<OtSterilizationModel>();
            }
        }

        public async Task<OtApiResponse> AddSterilizationAsync(AddOtSterilizationRequest req)
        {
            try
            {
                var res = await _http.PostAsJsonAsync("api/OT/sterilization/add", req);
                return await HandleResponseAsync(res);
            }
            catch (Exception ex)
            {
                return new OtApiResponse { message = $"Error: {ex.Message}" };
            }
        }

        public async Task<OtApiResponse> DeleteSterilizationAsync(Guid sterilizationId)
        {
            try
            {
                var res = await _http.GetAsync($"api/OT/sterilization/delete?sterilization_id={sterilizationId}");
                return await HandleResponseAsync(res);
            }
            catch (Exception ex)
            {
                return new OtApiResponse { message = $"Error: {ex.Message}" };
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // 17. OT SUMMARY (Read-only aggregate)
        // ═══════════════════════════════════════════════════════════════
        public async Task<OtSummaryModel?> GetSummaryAsync(Guid requestId)
        {
            try
            {
                return await _http.GetFromJsonAsync<OtSummaryModel>($"api/OT/summary/{requestId}", _jsonOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OtService.GetSummaryAsync] Error: {ex.Message}");
                return null;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // 18. CHECKLIST MASTER & TRANSACTION ITEMS
        // ═══════════════════════════════════════════════════════════════
        public async Task<OtApiResponse> AddChecklistMasterAsync(AddOtChecklistMasterRequest req)
        {
            try
            {
                var res = await _http.PostAsJsonAsync("api/OT/checklist-master/add", req);
                return await HandleResponseAsync(res);
            }
            catch (Exception ex)
            {
                return new OtApiResponse { message = $"Error: {ex.Message}" };
            }
        }

        public async Task<List<OtChecklistMasterModel>> GetChecklistMastersAsync(bool activeOnly = true)
        {
            try
            {
                var res = await _http.GetFromJsonAsync<List<OtChecklistMasterModel>>($"api/OT/checklist-master/list?activeOnly={activeOnly.ToString().ToLower()}", _jsonOptions);
                return res ?? new List<OtChecklistMasterModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OtService.GetChecklistMastersAsync] Error: {ex.Message}");
                return new List<OtChecklistMasterModel>();
            }
        }

        public async Task<OtApiResponse> SaveChecklistItemAsync(SaveOtChecklistItemRequest req)
        {
            try
            {
                var res = await _http.PostAsJsonAsync("api/OT/checklist/item/save", req);
                return await HandleResponseAsync(res);
            }
            catch (Exception ex)
            {
                return new OtApiResponse { message = $"Error: {ex.Message}" };
            }
        }

        public async Task<List<OtChecklistItemModel>> GetChecklistItemsByRequestAsync(Guid requestId)
        {
            try
            {
                var res = await _http.GetFromJsonAsync<List<OtChecklistItemModel>>($"api/OT/checklist/item/by-request?request_id={requestId}", _jsonOptions);
                return res ?? new List<OtChecklistItemModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OtService.GetChecklistItemsByRequestAsync] Error: {ex.Message}");
                return new List<OtChecklistItemModel>();
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // 19. OT AVAILABILITY SLOTS & RANGE
        // ═══════════════════════════════════════════════════════════════
        public async Task<OtAvailabilitySlotsResponse?> GetAvailabilitySlotsAsync(Guid otId, DateOnly date)
        {
            try
            {
                return await _http.GetFromJsonAsync<OtAvailabilitySlotsResponse>($"api/OT/availability/slots?ot_id={otId}&date={date:yyyy-MM-dd}", _jsonOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OtService.GetAvailabilitySlotsAsync] Error: {ex.Message}");
                return null;
            }
        }

        public async Task<OtAvailabilityRangeResponse?> GetAvailabilityRangeAsync(Guid otId, DateOnly fromDate, DateOnly toDate)
        {
            try
            {
                return await _http.GetFromJsonAsync<OtAvailabilityRangeResponse>($"api/OT/availability/range?ot_id={otId}&from_date={fromDate:yyyy-MM-dd}&to_date={toDate:yyyy-MM-dd}", _jsonOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OtService.GetAvailabilityRangeAsync] Error: {ex.Message}");
                return null;
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // 20. OT STAFF NOTES
        // ═══════════════════════════════════════════════════════════════
        public async Task<OtApiResponse> AddStaffNoteAsync(AddOtStaffNoteRequest req)
        {
            try
            {
                var res = await _http.PostAsJsonAsync("api/OT/staff-note/add", req);
                return await HandleResponseAsync(res);
            }
            catch (Exception ex)
            {
                return new OtApiResponse { message = $"Error: {ex.Message}" };
            }
        }

        public async Task<List<OtStaffNoteModel>> GetStaffNotesByRequestAsync(Guid requestId)
        {
            try
            {
                var res = await _http.GetFromJsonAsync<List<OtStaffNoteModel>>($"api/OT/staff-note/by-request?request_id={requestId}", _jsonOptions);
                return res ?? new List<OtStaffNoteModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OtService.GetStaffNotesByRequestAsync] Error: {ex.Message}");
                return new List<OtStaffNoteModel>();
            }
        }

        public async Task<OtApiResponse> DeleteStaffNoteAsync(Guid noteId)
        {
            try
            {
                var res = await _http.GetAsync($"api/OT/staff-note/delete?note_id={noteId}");
                return await HandleResponseAsync(res);
            }
            catch (Exception ex)
            {
                return new OtApiResponse { message = $"Error: {ex.Message}" };
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // 21. OT FEE SPLIT MASTER
        // ═══════════════════════════════════════════════════════════════
        public async Task<OtApiResponse> AddFeeSplitMasterAsync(AddOtFeeSplitMasterRequest req)
        {
            try
            {
                var res = await _http.PostAsJsonAsync("api/OT/fee-split-master/add", req);
                return await HandleResponseAsync(res);
            }
            catch (Exception ex)
            {
                return new OtApiResponse { message = $"Error: {ex.Message}" };
            }
        }

        public async Task<OtApiResponse> UpdateFeeSplitMasterAsync(UpdateOtFeeSplitMasterRequest req)
        {
            try
            {
                var res = await _http.PostAsJsonAsync("api/OT/fee-split-master/update", req);
                return await HandleResponseAsync(res);
            }
            catch (Exception ex)
            {
                return new OtApiResponse { message = $"Error: {ex.Message}" };
            }
        }

        public async Task<List<OtFeeSplitMasterModel>> GetFeeSplitMastersAsync(bool activeOnly = false)
        {
            try
            {
                var url = $"api/OT/fee-split-master/list?activeOnly={activeOnly.ToString().ToLower()}";
                var res = await _http.GetFromJsonAsync<List<OtFeeSplitMasterModel>>(url, _jsonOptions);
                return res ?? new List<OtFeeSplitMasterModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OtService.GetFeeSplitMastersAsync] Error: {ex.Message}");
                return new List<OtFeeSplitMasterModel>();
            }
        }

        public async Task<OtApiResponse> DeleteFeeSplitMasterAsync(Guid feeTypeId)
        {
            try
            {
                var res = await _http.GetAsync($"api/OT/fee-split-master/delete?fee_type_id={feeTypeId}");
                return await HandleResponseAsync(res);
            }
            catch (Exception ex)
            {
                return new OtApiResponse { message = $"Error: {ex.Message}" };
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // 22. OT FEE SPLIT ITEM & RECONCILIATION SUMMARY
        // ═══════════════════════════════════════════════════════════════
        public async Task<OtApiResponse> AddFeeSplitItemAsync(AddOtFeeSplitItemRequest req)
        {
            try
            {
                var res = await _http.PostAsJsonAsync("api/OT/fee-split/item/add", req);
                return await HandleResponseAsync(res);
            }
            catch (Exception ex)
            {
                return new OtApiResponse { message = $"Error: {ex.Message}" };
            }
        }

        public async Task<List<OtFeeSplitItemModel>> GetFeeSplitItemsByRequestAsync(Guid requestId)
        {
            try
            {
                var res = await _http.GetFromJsonAsync<List<OtFeeSplitItemModel>>($"api/OT/fee-split/item/by-request?request_id={requestId}", _jsonOptions);
                return res ?? new List<OtFeeSplitItemModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OtService.GetFeeSplitItemsByRequestAsync] Error: {ex.Message}");
                return new List<OtFeeSplitItemModel>();
            }
        }

        public async Task<OtApiResponse> DeleteFeeSplitItemAsync(Guid itemId)
        {
            try
            {
                var res = await _http.GetAsync($"api/OT/fee-split/item/delete?item_id={itemId}");
                return await HandleResponseAsync(res);
            }
            catch (Exception ex)
            {
                return new OtApiResponse { message = $"Error: {ex.Message}" };
            }
        }

        public async Task<OtFeeSplitSummaryModel?> GetFeeSplitSummaryAsync(Guid requestId)
        {
            try
            {
                return await _http.GetFromJsonAsync<OtFeeSplitSummaryModel>($"api/OT/fee-split/summary/{requestId}", _jsonOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OtService.GetFeeSplitSummaryAsync] Error: {ex.Message}");
                return null;
            }
        }
    }
}
