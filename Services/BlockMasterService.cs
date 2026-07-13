using System.Net.Http.Json;
using Medico_Backend.Model;

namespace Booking.Services
{
    public class BlockMasterService
    {
        private readonly HttpClient _http;

        public BlockMasterService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<BlockMasterModel>> GetBlockMastersAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<BlockMasterModel>>("api/BlockMaster/get");
                return response ?? new List<BlockMasterModel>();
            }
            catch
            {
                return new List<BlockMasterModel>();
            }
        }

        public async Task<bool> InsertBlockMasterAsync(BlockMasterModel block)
        {
            var response = await _http.PostAsJsonAsync("api/BlockMaster/insert", block);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateBlockMasterAsync(BlockMasterModel block)
        {
            var response = await _http.PostAsJsonAsync("api/BlockMaster/update", block);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteBlockMasterAsync(int blockcode)
        {
            var response = await _http.GetAsync($"api/BlockMaster/delete?blockcode={blockcode}");
            return response.IsSuccessStatusCode;
        }
    }
}
