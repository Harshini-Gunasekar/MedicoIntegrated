using System.Net.Http.Json;
using Booking.Models;

namespace Booking.Services
{
    public class DoctorService
    {
        private readonly HttpClient _http;

        public DoctorService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<DoctorMasterModel>> GetDoctorsAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<DoctorMasterModel>>("api/DoctorMaster/get");
                return response ?? new List<DoctorMasterModel>();
            }
            catch
            {
                return new List<DoctorMasterModel>();
            }
        }

        public async Task<DoctorMasterModel?> GetDoctorByCodeAsync(string dcode)
        {
            try
            {
                return await _http.GetFromJsonAsync<DoctorMasterModel>($"api/DoctorMaster/get-by-dcode?dcode={dcode}");
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> InsertDoctorAsync(DoctorMasterModel doctor, byte[]? imageBytes = null, string? imageName = null)
        {
            if (imageBytes != null && imageBytes.Length > 0 && !string.IsNullOrEmpty(imageName))
            {
                using var content = new MultipartFormDataContent();
                foreach (var prop in doctor.GetType().GetProperties())
                {
                    if (prop.Name == "doctorimage") continue;
                    var value = prop.GetValue(doctor);
                    if (value != null)
                    {
                        string stringValue = value is DateTime dt ? dt.ToString("yyyy-MM-ddTHH:mm:ss") : value.ToString() ?? "";
                        content.Add(new StringContent(stringValue), prop.Name);
                        content.Add(new StringContent(stringValue), $"DoctorMasterModel.{prop.Name}");
                        content.Add(new StringContent(stringValue), $"doctor.{prop.Name}");
                    }
                }

                var fileContent = new ByteArrayContent(imageBytes);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(GetMimeType(imageName));
                content.Add(fileContent, "doctorImageFile", imageName);
                content.Add(fileContent, "file", imageName);
                content.Add(fileContent, "doctorimage", imageName);
                content.Add(fileContent, "doctor_image", imageName);

                if (!string.IsNullOrEmpty(doctor.doctorimage))
                {
                    content.Add(new StringContent(doctor.doctorimage), "doctorimage");
                    content.Add(new StringContent(doctor.doctorimage), "DoctorMasterModel.doctorimage");
                    content.Add(new StringContent(doctor.doctorimage), "doctor.doctorimage");
                }

                var response = await _http.PostAsync("api/DoctorMaster/insert", content);
                return response.IsSuccessStatusCode;
            }
            else
            {
                var response = await _http.PostAsJsonAsync("api/DoctorMaster/insert", doctor);
                return response.IsSuccessStatusCode;
            }
        }

        public async Task<bool> UpdateDoctorAsync(DoctorMasterModel doctor, byte[]? imageBytes = null, string? imageName = null)
        {
            if (imageBytes != null && imageBytes.Length > 0 && !string.IsNullOrEmpty(imageName))
            {
                using var content = new MultipartFormDataContent();
                foreach (var prop in doctor.GetType().GetProperties())
                {
                    if (prop.Name == "doctorimage") continue;
                    var value = prop.GetValue(doctor);
                    if (value != null)
                    {
                        string stringValue = value is DateTime dt ? dt.ToString("yyyy-MM-ddTHH:mm:ss") : value.ToString() ?? "";
                        content.Add(new StringContent(stringValue), prop.Name);
                        content.Add(new StringContent(stringValue), $"DoctorMasterModel.{prop.Name}");
                        content.Add(new StringContent(stringValue), $"doctor.{prop.Name}");
                    }
                }

                var fileContent = new ByteArrayContent(imageBytes);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(GetMimeType(imageName));
                content.Add(fileContent, "doctorImageFile", imageName);
                content.Add(fileContent, "file", imageName);
                content.Add(fileContent, "doctorimage", imageName);
                content.Add(fileContent, "doctor_image", imageName);

                if (!string.IsNullOrEmpty(doctor.doctorimage))
                {
                    content.Add(new StringContent(doctor.doctorimage), "doctorimage");
                    content.Add(new StringContent(doctor.doctorimage), "DoctorMasterModel.doctorimage");
                    content.Add(new StringContent(doctor.doctorimage), "doctor.doctorimage");
                }

                var response = await _http.PostAsync($"api/DoctorMaster/update?dcode={doctor.dcode}", content);
                return response.IsSuccessStatusCode;
            }
            else
            {
                var response = await _http.PostAsJsonAsync($"api/DoctorMaster/update?dcode={doctor.dcode}", doctor);
                return response.IsSuccessStatusCode;
            }
        }

        public async Task<bool> DeleteDoctorAsync(string dcode)
        {
            var response = await _http.GetAsync($"api/DoctorMaster/delete?dcode={dcode}");
            return response.IsSuccessStatusCode;
        }

        public async Task<string?> DownloadFileAsBase64Async(string key)
        {
            try
            {
                var response = await _http.GetAsync($"api/files/download?key={Uri.EscapeDataString(key)}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonStr = await response.Content.ReadAsStringAsync();
                    var downloadRes = Newtonsoft.Json.JsonConvert.DeserializeObject<FileDownloadResponse>(jsonStr);
                    return downloadRes?.base64;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error downloading file with key {key}: {ex.Message}");
            }
            return null;
        }

        private string GetMimeType(string? fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return "image/png";
            string ext = System.IO.Path.GetExtension(fileName).ToLower();
            return ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".webp" => "image/webp",
                ".gif" => "image/gif",
                _ => "image/png"
            };
        }

        private class FileDownloadResponse
        {
            public string? fileName { get; set; }
            public string? base64 { get; set; }
        }
    }
}
