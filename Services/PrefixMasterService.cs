using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Medico_Backend.Model;

namespace Booking.Services
{
    public class PrefixMasterService
    {
        private readonly HttpClient _http;

        public PrefixMasterService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<PrefixMasterModel>> GetPrefixesAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<PrefixMasterModel>>("api/PrefixMaster/get");
                if (response != null && response.Any())
                {
                    return response.Where(p => p.deleted != true).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PrefixMasterService] Error fetching prefixes from api/PrefixMaster/get: {ex.Message}");
            }
            return new List<PrefixMasterModel>();
        }
    }
}
