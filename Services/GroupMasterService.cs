using System.Net.Http.Json;
using Booking.Models;

namespace Booking.Services
{
    public class GroupMasterService
    {
        private readonly HttpClient _http;

        public GroupMasterService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<GroupMasterModel>> GetGroupMastersAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<GroupMasterModel>>("api/GroupMaster/get");
                return response ?? new List<GroupMasterModel>();
            }
            catch
            {
                return new List<GroupMasterModel>();
            }
        }

        public async Task<bool> InsertGroupMasterAsync(GroupMasterModel group)
        {
            var response = await _http.PostAsJsonAsync("api/GroupMaster/insert", group);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateGroupMasterAsync(GroupMasterModel group)
        {
            var response = await _http.PostAsJsonAsync("api/GroupMaster/update", group);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteGroupMasterAsync(decimal gcode)
        {
            var response = await _http.GetAsync($"api/GroupMaster/delete?gcode={gcode}");
            return response.IsSuccessStatusCode;
        }
    }
}
