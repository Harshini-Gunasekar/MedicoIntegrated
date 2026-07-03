 using LIMS_Backend.Model;
using SharedComponents.Rcl.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace LabCare.Services
{
    public class UserService
    {
        private readonly HttpClient _http;
        private readonly TenantSessionState _session;

        private static readonly Newtonsoft.Json.JsonSerializerSettings _userJsonSettings = new()
        {
            MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore,
            NullValueHandling     = Newtonsoft.Json.NullValueHandling.Ignore
        };

        public UserService(HttpClient http, TenantSessionState session)
        {
            _http = http;
            _session = session;
        }

        private void ConfigureHeaders()
        {
            if (!string.IsNullOrEmpty(_session.AuthToken))
            {
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _session.AuthToken);
            }
            if (!string.IsNullOrEmpty(_session.TenantCode))
            {
                if (_http.DefaultRequestHeaders.Contains("tenant_code")) 
                    _http.DefaultRequestHeaders.Remove("tenant_code");
                _http.DefaultRequestHeaders.Add("tenant_code", _session.TenantCode);
            }
        }

        public async Task<GetUser?> GetAsync(int userCode)
        {
            ConfigureHeaders();
            try
            {
                var response = await _http.GetAsync($"api/user/get?user_code={userCode}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonStr = await response.Content.ReadAsStringAsync();
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<GetUser>(jsonStr, _userJsonSettings);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[UserService.GetAsync] Error: {ex.Message}");
            }
            return null;
        }

        public async Task<List<UserResponseWrapper>> GetAllAsync()
        {
            ConfigureHeaders();
            try
            {
                var response = await _http.GetAsync("api/user/getall");
                if (response.IsSuccessStatusCode)
                {
                    var jsonStr = await response.Content.ReadAsStringAsync();
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<List<UserResponseWrapper>>(jsonStr, _userJsonSettings) ?? new();
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[UserService.GetAllAsync] Error: {ex.Message}");
            }
            return new();
        }

        private void LogPayloadToTerminal(string actionName, user_master user, IList<UserBranchMaster>? branches, string? userImageFileName, string? signatureImageFileName)
        {
            try
            {
                var payloadObject = new
                {
                    Action = actionName,
                    User = user,
                    Branches = branches ?? new List<UserBranchMaster>(),
                    UserImage = userImageFileName,
                    SignatureImage = signatureImageFileName
                };
                
                string jsonString = System.Text.Json.JsonSerializer.Serialize(payloadObject, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });
                
                Console.WriteLine($"=== [UserService] POST PAYLOAD FOR {actionName} ===");
                Console.WriteLine(jsonString);
                Console.WriteLine("==================================================");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[UserService.LogPayloadToTerminal] Error serializing payload: {ex.Message}");
            }
        }

        public async Task<HttpResponseMessage> InsertAsync(
            user_master model, 
            IList<UserBranchMaster>? branchModel = null, 
            byte[]? userImageFile = null, 
            string? userImageFileName = null, 
            byte[]? signatureImageFile = null, 
            string? signatureImageFileName = null)
        {
            ConfigureHeaders();
            LogPayloadToTerminal("Insert", model, branchModel, userImageFileName, signatureImageFileName);
            using var content = BuildUserFormData(model, branchModel, userImageFile, userImageFileName, signatureImageFile, signatureImageFileName, "model");
            return await _http.PostAsync("api/user/insert", content);
        }

        public async Task<HttpResponseMessage> RegisterAsync(
            user_master model, 
            IList<UserBranchMaster>? branchModel = null, 
            byte[]? userImageFile = null, 
            string? userImageFileName = null, 
            byte[]? signatureImageFile = null, 
            string? signatureImageFileName = null)
        {
            ConfigureHeaders();
            LogPayloadToTerminal("Register", model, branchModel, userImageFileName, signatureImageFileName);
            using var content = BuildUserFormData(model, branchModel, userImageFile, userImageFileName, signatureImageFile, signatureImageFileName, "model");
            return await _http.PostAsync("api/user/register", content);
        }

        public async Task<HttpResponseMessage> UpdateAsync(
            user_master user, 
            IList<UserBranchMaster>? branchModel = null, 
            byte[]? userImageFile = null, 
            string? userImageFileName = null, 
            byte[]? signatureImageFile = null, 
            string? signatureImageFileName = null)
        {
            ConfigureHeaders();
            LogPayloadToTerminal("Update", user, branchModel, userImageFileName, signatureImageFileName);
            using var content = BuildUserFormData(user, branchModel, userImageFile, userImageFileName, signatureImageFile, signatureImageFileName, "user");
            return await _http.PostAsync("api/user/update", content);
        }

        public async Task<HttpResponseMessage> SoftDeleteAsync(int userCode)
        {
            ConfigureHeaders();
            return await _http.GetAsync($"api/user/softdelete?user_code={userCode}");
        }

        public async Task<HttpResponseMessage> PermanentDeleteAsync(int userCode)
        {
            ConfigureHeaders();
            return await _http.GetAsync($"api/user/permanentdelete?user_code={userCode}");
        }

        public async Task<HttpResponseMessage> VerifyAsync(int userCode)
        {
            ConfigureHeaders();
            return await _http.GetAsync($"api/user/verify?user_code={userCode}");
        }

        public async Task<List<string>> GetRolesAsync()
        {
            ConfigureHeaders();
            try
            {
                return await _http.GetFromJsonAsync<List<string>>("api/user/roles") ?? new();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[UserService.GetRolesAsync] Error: {ex.Message}");
                return new();
            }
        }

        public async Task<HttpResponseMessage> LoginAsync(LoginDto dto)
        {
            // Login does not require tenant code header pre-authentication, but configure standard settings
            return await _http.PostAsJsonAsync("api/user/login", dto);
        }

        private MultipartFormDataContent BuildUserFormData(
            user_master user, 
            IList<UserBranchMaster>? branchModel, 
            byte[]? userImageBytes, 
            string? userImageName, 
            byte[]? signatureImageBytes, 
            string? signatureImageName,
            string modelPrefix)
        {
            var content = new MultipartFormDataContent();

            Action<string, string> addField = (name, value) => {
                content.Add(new StringContent(value), name);
                content.Add(new StringContent(value), $"user.{name}");
                content.Add(new StringContent(value), $"model.{name}");
                if (name == "user_code")
                {
                    content.Add(new StringContent(value), "usercode");
                    content.Add(new StringContent(value), "userCode");
                    content.Add(new StringContent(value), "user.usercode");
                    content.Add(new StringContent(value), "user.userCode");
                    content.Add(new StringContent(value), "model.usercode");
                    content.Add(new StringContent(value), "model.userCode");
                }
            };

            foreach (var prop in user.GetType().GetProperties())
            {
                if (prop.Name == "user_image" || prop.Name == "signature_image") continue;
                
                var value = prop.GetValue(user);
                if (value != null)
                {
                    string stringValue = value is DateTime dt ? dt.ToString("yyyy-MM-ddTHH:mm:ss") : value.ToString() ?? "";
                    addField(prop.Name, stringValue);
                }
            }

            // Pass-through existing filenames if no new files are uploaded
            if (!string.IsNullOrEmpty(user.user_image))
            {
                addField("user_image", user.user_image);
            }
            if (!string.IsNullOrEmpty(user.signature_image))
            {
                addField("signature_image", user.signature_image);
            }

            // Branches
            if (branchModel != null && branchModel.Count > 0)
            {
                for (int i = 0; i < branchModel.Count; i++)
                {
                    var branch = branchModel[i];
                    if (branch.bhcode.HasValue)
                    {
                        content.Add(new StringContent(branch.bhcode.Value.ToString()), $"branch[{i}].bhcode");
                        content.Add(new StringContent(branch.bhcode.Value.ToString()), $"branchModel[{i}].bhcode");
                    }
                    if (branch.cntcode.HasValue)
                    {
                        content.Add(new StringContent(branch.cntcode.Value.ToString()), $"branch[{i}].cntcode");
                        content.Add(new StringContent(branch.cntcode.Value.ToString()), $"branchModel[{i}].cntcode");
                    }
                    if (branch.user_code.HasValue)
                    {
                        content.Add(new StringContent(branch.user_code.Value.ToString()), $"branch[{i}].user_code");
                        content.Add(new StringContent(branch.user_code.Value.ToString()), $"branch[{i}].usercode");
                        content.Add(new StringContent(branch.user_code.Value.ToString()), $"branch[{i}].userCode");
                        content.Add(new StringContent(branch.user_code.Value.ToString()), $"branchModel[{i}].user_code");
                        content.Add(new StringContent(branch.user_code.Value.ToString()), $"branchModel[{i}].usercode");
                        content.Add(new StringContent(branch.user_code.Value.ToString()), $"branchModel[{i}].userCode");
                    }
                    if (!string.IsNullOrEmpty(branch.tenant_code))
                    {
                        content.Add(new StringContent(branch.tenant_code), $"branch[{i}].tenant_code");
                        content.Add(new StringContent(branch.tenant_code), $"branchModel[{i}].tenant_code");
                    }
                    content.Add(new StringContent(branch.deleted.ToString().ToLower()), $"branch[{i}].deleted");
                    content.Add(new StringContent(branch.deleted.ToString().ToLower()), $"branchModel[{i}].deleted");
                }
            }

            // Files
            if (userImageBytes != null && userImageBytes.Length > 0 && !string.IsNullOrEmpty(userImageName))
            {
                var fileContent = new ByteArrayContent(userImageBytes);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(GetMimeType(userImageName));
                content.Add(fileContent, "userImageFile", userImageName);
            }
            if (signatureImageBytes != null && signatureImageBytes.Length > 0 && !string.IsNullOrEmpty(signatureImageName))
            {
                var fileContent = new ByteArrayContent(signatureImageBytes);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(GetMimeType(signatureImageName));
                content.Add(fileContent, "signatureImageFile", signatureImageName);
            }

            return content;
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
    }
}
