using System.Net.Http.Json;
using Medico_Backend.Model;

namespace Booking.Services
{
    public class RoomTypeMasterService
    {
        private readonly HttpClient _http;

        public RoomTypeMasterService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<RoomTypeMasterModel>> GetRoomTypeMastersAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<RoomTypeMasterModel>>("api/RoomTypeMaster/get");
                return response ?? new List<RoomTypeMasterModel>();
            }
            catch
            {
                return new List<RoomTypeMasterModel>();
            }
        }

        public async Task<bool> InsertRoomTypeMasterAsync(RoomTypeMasterModel roomType)
        {
            var response = await _http.PostAsJsonAsync("api/RoomTypeMaster/insert", roomType);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateRoomTypeMasterAsync(RoomTypeMasterModel roomType)
        {
            var response = await _http.PostAsJsonAsync("api/RoomTypeMaster/update", roomType);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteRoomTypeMasterAsync(int rmtcode)
        {
            var response = await _http.GetAsync($"api/RoomTypeMaster/delete?rmtcode={rmtcode}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<TestGroupRateModel>> GetTestGroupRatesByRmtCodeAsync(int rmtcode)
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<TestGroupRateModel>>($"api/TestGroupRate/get-by-rmtcode?rmtcode={rmtcode}");
                return response ?? new List<TestGroupRateModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RoomTypeMasterService] Error in GetTestGroupRatesByRmtCodeAsync: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                return new List<TestGroupRateModel>();
            }
        }

        public async Task<bool> SaveTestGroupRatesForRoomTypeAsync(SaveRoomTypeRatesRequest request)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/TestGroupRate/save-for-roomtype", request);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteTestGroupRateAsync(int id)
        {
            try
            {
                var response = await _http.GetAsync($"api/TestGroupRate/delete?id={id}");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
