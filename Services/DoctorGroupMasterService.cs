using System.Net.Http.Json;
using Medico_Backend.Model;

namespace Booking.Services
{
    public class DoctorGroupMasterService
    {
        private readonly HttpClient _http;

        public DoctorGroupMasterService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<DoctorGroupMasterModel>> GetDoctorGroupsAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<DoctorGroupMasterModel>>("api/DoctorGroupMaster/get");
                return response ?? new List<DoctorGroupMasterModel>();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error getting Doctor Groups: {ex.Message}");
                return new List<DoctorGroupMasterModel>();
            }
        }

        public async Task<bool> InsertDoctorGroupAsync(DoctorGroupMasterModel model)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/DoctorGroupMaster/insert", model);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error inserting Doctor Group: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateDoctorGroupAsync(DoctorGroupMasterModel model)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/DoctorGroupMaster/update", model);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error updating Doctor Group: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteDoctorGroupAsync(long group_id)
        {
            try
            {
                var response = await _http.GetAsync($"api/DoctorGroupMaster/delete?group_id={group_id}");
                if (!response.IsSuccessStatusCode)
                {
                    response = await _http.DeleteAsync($"api/DoctorGroupMaster/delete?group_id={group_id}");
                }
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error deleting Doctor Group #{group_id}: {ex.Message}");
                return false;
            }
        }
    }
}
