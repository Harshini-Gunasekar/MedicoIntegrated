using System.Net.Http.Json;
using Medico_Backend.Model;

namespace Booking.Services
{
    public class OgQueueFrontendDto
    {
        public int? ogentryid { get; set; }
        public int? vitalentryid { get; set; }
        public string? tenant_code { get; set; }
        public string? og_token_no { get; set; }
        public string? token_no { get; set; }
        public string? custcode { get; set; }
        public int? dcode { get; set; }
        public TimeOnly? arrival_time { get; set; }
        public string? entry_type { get; set; }
        public TimeOnly? out_time { get; set; }
        public string? notes { get; set; }
        
        public string? vitals_status { get; set; }
        public string? queue_status { get; set; }

        private string? _status;
        public string? status 
        { 
            get => !string.IsNullOrWhiteSpace(queue_status) ? queue_status : (_status ?? vitals_status); 
            set 
            {
                _status = value;
                queue_status = value;
            }
        }

        public int? usercode { get; set; }
        public int? computercode { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public bool? deleted { get; set; }

        // Extra DTO fields
        public string? name { get; set; }
        public string? patient_name { get; set; }
        public string? customer_name { get; set; }
        public string? custname { get; set; }
        public int? group_id { get; set; }
        public string? group_name { get; set; }
        public string? doctor_name { get; set; }
    }

    public class OGScreenService
    {
        private readonly HttpClient _http;

        public OGScreenService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<OgQueueFrontendDto>> GetConsultationListAsync(string date)
        {
            try
            {
                var response = await _http.GetAsync($"api/OgQueue/consultation-list?date={date}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<List<OgQueueFrontendDto>>(json) ?? new List<OgQueueFrontendDto>();
                }
                return new List<OgQueueFrontendDto>();
            }
            catch (ObjectDisposedException)
            {
                return new List<OgQueueFrontendDto>(); // Suppress exception silently on disposal
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OGScreenService] Error loading consultation list: {ex.Message}");
                return new List<OgQueueFrontendDto>();
            }
        }

        public async Task<List<OgQueueFrontendDto>> GetLabScanListAsync(string date)
        {
            try
            {
                var response = await _http.GetAsync($"api/OgQueue/lab-scan-list?date={date}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<List<OgQueueFrontendDto>>(json) ?? new List<OgQueueFrontendDto>();
                }
                return new List<OgQueueFrontendDto>();
            }
            catch (ObjectDisposedException)
            {
                return new List<OgQueueFrontendDto>(); // Suppress exception silently on disposal
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OGScreenService] Error loading lab scan list: {ex.Message}");
                return new List<OgQueueFrontendDto>();
            }
        }

        public async Task<bool> UpdateStatusAsync(UpdateOgStatusRequest request)
        {
            try
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(request);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await _http.PostAsync("api/OgQueue/update-status", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OGScreenService] Error updating status: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateOutTimeAsync(UpdateOgOutTimeRequest request)
        {
            try
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(request);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await _http.PostAsync("api/OgQueue/update-out-time", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OGScreenService] Error updating out time: {ex.Message}");
                return false;
            }
        }
    }
}
