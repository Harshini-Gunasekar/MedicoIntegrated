using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Medico_Backend.Model;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Headers;

namespace Booking.Services
{
    public class DoctorProfileService
    {
        private readonly HttpClient _http;

        public DoctorProfileService(HttpClient http)
        {
            _http = http;
        }

        public async Task<DoctorProfileModel?> GetProfileAsync(int dcode)
        {
            try
            {
                var response = await _http.GetFromJsonAsync<System.Collections.Generic.List<DoctorProfileModel>>($"api/DoctorProfile/get?dcode={dcode}");
                return response != null && response.Count > 0 ? response[0] : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching doctor profile: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> InsertProfileAsync(DoctorProfileModel profile, IBrowserFile? bannerImageFile)
        {
            try
            {
                using var content = new MultipartFormDataContent();
                
                AddProfilePropertiesToMultipart(content, profile);

                if (bannerImageFile != null)
                {
                    var fileStream = bannerImageFile.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024); // 10MB max
                    var streamContent = new StreamContent(fileStream);
                    streamContent.Headers.ContentType = new MediaTypeHeaderValue(bannerImageFile.ContentType);
                    content.Add(streamContent, "bannerImageFile", bannerImageFile.Name);
                }

                var response = await _http.PostAsync("api/DoctorProfile/insert", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting doctor profile: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateProfileAsync(DoctorProfileModel profile, IBrowserFile? bannerImageFile)
        {
            try
            {
                using var content = new MultipartFormDataContent();
                
                AddProfilePropertiesToMultipart(content, profile);

                if (bannerImageFile != null)
                {
                    var fileStream = bannerImageFile.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024); // 10MB max
                    var streamContent = new StreamContent(fileStream);
                    streamContent.Headers.ContentType = new MediaTypeHeaderValue(bannerImageFile.ContentType);
                    content.Add(streamContent, "bannerImageFile", bannerImageFile.Name);
                }

                var response = await _http.PostAsync("api/DoctorProfile/update", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating doctor profile: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteProfileAsync(int pcode)
        {
            try
            {
                var response = await _http.GetAsync($"api/DoctorProfile/delete?pcode={pcode}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting doctor profile: {ex.Message}");
                return false;
            }
        }

        private void AddProfilePropertiesToMultipart(MultipartFormDataContent content, DoctorProfileModel profile)
        {
            if (profile.pcode > 0) content.Add(new StringContent(profile.pcode.ToString()), "pcode");
            content.Add(new StringContent(profile.dcode.ToString()), "dcode");
            
            if (profile.about != null) content.Add(new StringContent(profile.about), "about");
            if (profile.experience_years.HasValue) content.Add(new StringContent(profile.experience_years.Value.ToString()), "experience_years");
            if (profile.education_details != null) content.Add(new StringContent(profile.education_details), "education_details");
            if (profile.operations_performed != null) content.Add(new StringContent(profile.operations_performed), "operations_performed");
            if (profile.patients_treated.HasValue) content.Add(new StringContent(profile.patients_treated.Value.ToString()), "patients_treated");
            if (profile.achievements != null) content.Add(new StringContent(profile.achievements), "achievements");
            if (profile.memberships != null) content.Add(new StringContent(profile.memberships), "memberships");
            if (profile.publications != null) content.Add(new StringContent(profile.publications), "publications");
            if (profile.languages_known != null) content.Add(new StringContent(profile.languages_known), "languages_known");
            if (profile.profile_video_url != null) content.Add(new StringContent(profile.profile_video_url), "profile_video_url");
            if (profile.banner_image != null) content.Add(new StringContent(profile.banner_image), "banner_image");
            if (profile.orderno.HasValue) content.Add(new StringContent(profile.orderno.Value.ToString()), "orderno");
            
            content.Add(new StringContent(profile.is_published.ToString()), "is_published");
            content.Add(new StringContent(profile.deleted.ToString()), "deleted");
            
            if (profile.usercode.HasValue) content.Add(new StringContent(profile.usercode.Value.ToString()), "usercode");
            if (profile.computercode.HasValue) content.Add(new StringContent(profile.computercode.Value.ToString()), "computercode");
            if (profile.tenant_code != null) content.Add(new StringContent(profile.tenant_code), "tenant_code");
        }
    }
}
