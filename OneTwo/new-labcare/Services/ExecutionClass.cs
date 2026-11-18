using System.Net.Http.Json;

namespace LabCare.Services
{
    public class ExecutionClass
    {
        private readonly HttpClient _http;

        public ExecutionClass(HttpClient http)
        {
            _http = http;
        }

        public async Task<string> GetStatement(object parameters, string endpoint)
        {
            var response = await _http.PostAsJsonAsync(endpoint, parameters);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            return string.Empty;
        }
    }
}
