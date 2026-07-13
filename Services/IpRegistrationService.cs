using System.Net.Http.Json;
using medico_backend.Model;

namespace Booking.Services
{
    public class IpRegistrationService
    {
        private readonly HttpClient _http;

        public IpRegistrationService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<IPRegistrationModel.IpRegistrationModel>> GetIpRegistrationsAsync(string status)
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<IPRegistrationModel.IpRegistrationModel>>($"api/IpRegistration/get?ip_status={status}");
                return response ?? new List<IPRegistrationModel.IpRegistrationModel>();
            }
            catch
            {
                return new List<IPRegistrationModel.IpRegistrationModel>();
            }
        }

        public async Task<List<IPRegistrationModel.IpRegistrationModel>> GetActiveAdmissionsAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<IPRegistrationModel.IpRegistrationModel>>("api/IpRegistration/active-admissions");
                return response ?? new List<IPRegistrationModel.IpRegistrationModel>();
            }
            catch
            {
                return new List<IPRegistrationModel.IpRegistrationModel>();
            }
        }

        public async Task<IPRegistrationModel.IpRegistrationModel?> GetIpRegistrationByIdAsync(Guid ipId)
        {
            try
            {
                var response = await _http.GetFromJsonAsync<IPRegistrationModel.IpRegistrationModel>($"api/IpRegistration/get-by-id?ip_id={ipId}");
                return response;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> AdmitPatientAsync(IPRegistrationModel.CreateIpRegistrationRequest request)
        {
            var response = await _http.PostAsJsonAsync("api/IpRegistration/admit", request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DischargePatientAsync(IPRegistrationModel.DischargeRequest request)
        {
            var response = await _http.PostAsJsonAsync("api/IpRegistration/discharge", request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateIpRegistrationAsync(IPRegistrationModel.UpdateIpRegistrationRequest request, decimal? custid = null)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(request);
            var dict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(json) ?? new();
            if (custid.HasValue)
            {
                dict["custid"] = custid.Value;
            }
            var response = await _http.PostAsJsonAsync("api/IpRegistration/update", dict);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> CancelAdmissionAsync(IPRegistrationModel.CancelAdmissionRequest request)
        {
            var response = await _http.PostAsJsonAsync("api/IpRegistration/cancel", request);
            return response.IsSuccessStatusCode;
        }
    }
}
