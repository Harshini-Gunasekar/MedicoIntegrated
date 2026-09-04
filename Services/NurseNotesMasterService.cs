using System.Net.Http.Json;
using medico_backend.Model;

namespace Booking.Services
{
    public class NurseNotesMasterService
    {
        private readonly HttpClient _http;

        public NurseNotesMasterService(HttpClient http)
        {
            _http = http;
        }

        // ═══════════════════════════════════════
        // 1. IO Particulars Master
        // ═══════════════════════════════════════
        public async Task<List<IoParticularsMasterModel>> GetIoParticularsAsync(bool activeOnly = false)
        {
            try
            {
                var res = await _http.GetFromJsonAsync<List<IoParticularsMasterModel>>($"api/MasterList/io-particulars?activeOnly={activeOnly.ToString().ToLower()}");
                return res ?? new List<IoParticularsMasterModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetIoParticularsAsync] Error: {ex.Message}");
                return new List<IoParticularsMasterModel>();
            }
        }

        public async Task<bool> AddIoParticularAsync(AddIoParticularRequest request)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/MasterList/io-particulars/add", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AddIoParticularAsync] Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateIoParticularAsync(UpdateIoParticularRequest request)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/MasterList/io-particulars/update", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UpdateIoParticularAsync] Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteIoParticularAsync(int particularId)
        {
            try
            {
                var response = await _http.GetAsync($"api/MasterList/io-particulars/delete?particular_id={particularId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DeleteIoParticularAsync] Error: {ex.Message}");
                return false;
            }
        }

        // ═══════════════════════════════════════
        // 2. Service Name Master
        // ═══════════════════════════════════════
        public async Task<List<ServiceNameMasterModel>> GetServiceNamesAsync(bool activeOnly = false)
        {
            try
            {
                var res = await _http.GetFromJsonAsync<List<ServiceNameMasterModel>>($"api/MasterList/service-name?activeOnly={activeOnly.ToString().ToLower()}");
                return res ?? new List<ServiceNameMasterModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetServiceNamesAsync] Error: {ex.Message}");
                return new List<ServiceNameMasterModel>();
            }
        }

        public async Task<bool> AddServiceNameAsync(AddServiceNameRequest request)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/MasterList/service-name/add", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AddServiceNameAsync] Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateServiceNameAsync(UpdateServiceNameRequest request)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/MasterList/service-name/update", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UpdateServiceNameAsync] Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteServiceNameAsync(int serviceId)
        {
            try
            {
                var response = await _http.GetAsync($"api/MasterList/service-name/delete?service_id={serviceId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DeleteServiceNameAsync] Error: {ex.Message}");
                return false;
            }
        }

        // ═══════════════════════════════════════
        // 3. Schedule Type Master
        // ═══════════════════════════════════════
        public async Task<List<ScheduleTypeMasterModel>> GetScheduleTypesAsync(bool activeOnly = false)
        {
            try
            {
                var res = await _http.GetFromJsonAsync<List<ScheduleTypeMasterModel>>($"api/MasterList/schedule-type?activeOnly={activeOnly.ToString().ToLower()}");
                return res ?? new List<ScheduleTypeMasterModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetScheduleTypesAsync] Error: {ex.Message}");
                return new List<ScheduleTypeMasterModel>();
            }
        }

        public async Task<bool> AddScheduleTypeAsync(AddScheduleTypeRequest request)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/MasterList/schedule-type/add", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AddScheduleTypeAsync] Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateScheduleTypeAsync(UpdateScheduleTypeRequest request)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/MasterList/schedule-type/update", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UpdateScheduleTypeAsync] Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteScheduleTypeAsync(int typeId)
        {
            try
            {
                var response = await _http.GetAsync($"api/MasterList/schedule-type/delete?type_id={typeId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DeleteScheduleTypeAsync] Error: {ex.Message}");
                return false;
            }
        }
    }
}
