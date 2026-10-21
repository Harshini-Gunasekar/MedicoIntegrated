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

        public void ConfigureHeaders(string? token = null, string? tenantCode = null)
        {
            string effToken = !string.IsNullOrEmpty(token) ? token : _session.AuthToken;
            string effTenant = !string.IsNullOrEmpty(tenantCode) ? tenantCode : _session.TenantCode;

            if (!string.IsNullOrEmpty(effToken))
            {
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", effToken);
            }
            if (!string.IsNullOrEmpty(effTenant))
            {
                if (_http.DefaultRequestHeaders.Contains("tenant_code")) 
                    _http.DefaultRequestHeaders.Remove("tenant_code");
                _http.DefaultRequestHeaders.Add("tenant_code", effTenant);
            }
        }

        public async Task<GetUser?> GetAsync(int userCode, string? token = null, string? tenantCode = null)
        {
            ConfigureHeaders(token, tenantCode);
            try
            {
                var response = await _http.GetAsync($"api/user/get?user_code={userCode}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonStr = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrWhiteSpace(jsonStr))
                    {
                        var trimmed = jsonStr.TrimStart();
                        if (trimmed.StartsWith("["))
                        {
                            try
                            {
                                var list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<GetUser>>(jsonStr, _userJsonSettings);
                                if (list != null && list.Any() && list[0]?.user != null)
                                {
                                    return list.First();
                                }
                            }
                            catch { }

                            try
                            {
                                var flatList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<user_master>>(jsonStr, _userJsonSettings);
                                if (flatList != null && flatList.Any())
                                {
                                    return new GetUser { user = flatList.First() };
                                }
                            }
                            catch { }
                        }
                        else
                        {
                            try
                            {
                                var parsed = Newtonsoft.Json.JsonConvert.DeserializeObject<GetUser>(jsonStr, _userJsonSettings);
                                if (parsed?.user != null && (!string.IsNullOrEmpty(parsed.user.name) || parsed.user.user_code > 0))
                                {
                                    return parsed;
                                }
                            }
                            catch { }

                            try
                            {
                                var flat = Newtonsoft.Json.JsonConvert.DeserializeObject<user_master>(jsonStr, _userJsonSettings);
                                if (flat != null && (!string.IsNullOrEmpty(flat.name) || !string.IsNullOrEmpty(flat.short_name) || flat.user_code > 0))
                                {
                                    return new GetUser { user = flat };
                                }
                            }
                            catch { }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[UserService.GetAsync] Error: {ex.Message}");
            }
            return null;
        }

        public async Task<user_master?> GetUserMasterAsync(int userCode, string? token = null, string? tenantCode = null)
        {
            var res = await GetAsync(userCode, token, tenantCode);
            if (res?.user != null) return res.user;

            try
            {
                var all = await GetAllAsync(token, tenantCode);
                var found = all.FirstOrDefault(u => u.user != null && (u.user.user_code == userCode || u.user.usercode == userCode));
                if (found?.user != null) return found.user;
            }
            catch { }
            return null;
        }

        public async Task<List<GetUser>> GetAllAsync(string? token = null, string? tenantCode = null)
        {
            ConfigureHeaders(token, tenantCode);
            try
            {
                var response = await _http.GetAsync("api/user/getall");
                if (response.IsSuccessStatusCode)
                {
                    var jsonStr = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrWhiteSpace(jsonStr))
                    {
                        var trimmed = jsonStr.TrimStart();
                        if (trimmed.StartsWith("["))
                        {
                            try
                            {
                                var list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<GetUser>>(jsonStr, _userJsonSettings);
                                if (list != null && list.Any() && list.Any(u => u?.user != null))
                                {
                                    return list;
                                }
                            }
                            catch { }

                            try
                            {
                                var flatList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<user_master>>(jsonStr, _userJsonSettings);
                                if (flatList != null && flatList.Any())
                                {
                                    return flatList.Select(u => new GetUser { user = u }).ToList();
                                }
                            }
                            catch { }
                        }
                    }
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

        private void EnsureBranchCountersSynced(user_master model, IList<UserBranchMaster>? branchModel)
        {
            if (branchModel == null || branchModel.Count == 0) return;

            // If user has a selected counter, make sure the matching branch in branchModel has it
            if (model.cnt_code.HasValue && model.cnt_code.Value > 0)
            {
                var targetBranch = branchModel.FirstOrDefault(b => b.bhcode == model.bh_code) 
                                ?? (branchModel.Count == 1 ? branchModel[0] : null);
                if (targetBranch != null && !targetBranch.cntcode.HasValue)
                {
                    targetBranch.cntcode = (int)model.cnt_code.Value;
                }
            }
        }

        public async Task<HttpResponseMessage> InsertAsync(
            user_master model, 
            IList<UserBranchMaster>? branchModel = null, 
            IList<UserDepartmentMaster>? departmentModel = null,
            byte[]? userImageFile = null, 
            string? userImageFileName = null, 
            byte[]? signatureImageFile = null, 
            string? signatureImageFileName = null,
            IList<long>? roleIds = null,
            string? roleTemplateName = null,
            Guid? roleTemplateGuid = null)
        {
            ConfigureHeaders();
            EnsureBranchCountersSynced(model, branchModel);
            LogPayloadToTerminal("Insert", model, branchModel, userImageFileName, signatureImageFileName);
            using var content = BuildUserFormData(model, branchModel, departmentModel, userImageFile, userImageFileName, signatureImageFile, signatureImageFileName, isUpdate: false, roleIds, roleTemplateName, roleTemplateGuid);
            return await _http.PostAsync("api/user/insert", content);
        }

        public async Task<HttpResponseMessage> RegisterAsync(
            user_master model, 
            IList<UserBranchMaster>? branchModel = null, 
            IList<UserDepartmentMaster>? departmentModel = null,
            byte[]? userImageFile = null, 
            string? userImageFileName = null, 
            byte[]? signatureImageFile = null, 
            string? signatureImageFileName = null,
            bool isAnonymousRegister = false,
            IList<long>? roleIds = null,
            string? roleTemplateName = null,
            Guid? roleTemplateGuid = null)
        {
            EnsureBranchCountersSynced(model, branchModel);
            if (isAnonymousRegister)
            {
                LogPayloadToTerminal("Register", model, branchModel, userImageFileName, signatureImageFileName);
                var content = BuildUserFormData(model, branchModel, departmentModel, userImageFile, userImageFileName, signatureImageFile, signatureImageFileName, isUpdate: false, roleIds, roleTemplateName, roleTemplateGuid);
                
                var request = new HttpRequestMessage(HttpMethod.Post, "api/user/register")
                {
                    Content = content
                };

                if (!string.IsNullOrEmpty(model.tenant_code))
                {
                    request.Headers.Add("tenant_code", model.tenant_code);
                }

                request.Headers.Add("X-Anonymous-Register", "true");

                return await _http.SendAsync(request);
            }
            else
            {
                ConfigureHeaders();
                LogPayloadToTerminal("Register", model, branchModel, userImageFileName, signatureImageFileName);
                using var content = BuildUserFormData(model, branchModel, departmentModel, userImageFile, userImageFileName, signatureImageFile, signatureImageFileName, isUpdate: false, roleIds, roleTemplateName, roleTemplateGuid);
                return await _http.PostAsync("api/user/register", content);
            }
        }

        public async Task<HttpResponseMessage> UpdateAsync(
            user_master user, 
            IList<UserBranchMaster>? branchModel = null, 
            IList<UserDepartmentMaster>? departmentModel = null,
            byte[]? userImageFile = null, 
            string? userImageFileName = null, 
            byte[]? signatureImageFile = null, 
            string? signatureImageFileName = null,
            IList<long>? roleIds = null,
            string? roleTemplateName = null,
            Guid? roleTemplateGuid = null)
        {
            ConfigureHeaders();
            EnsureBranchCountersSynced(user, branchModel);
            LogPayloadToTerminal("Update", user, branchModel, userImageFileName, signatureImageFileName);
            using var content = BuildUserFormData(user, branchModel, departmentModel, userImageFile, userImageFileName, signatureImageFile, signatureImageFileName, isUpdate: true, roleIds, roleTemplateName, roleTemplateGuid);
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
            IList<UserDepartmentMaster>? departmentModel,
            byte[]? userImageBytes, 
            string? userImageName, 
            byte[]? signatureImageBytes, 
            string? signatureImageName,
            bool isUpdate,
            IList<long>? roleIds = null,
            string? roleTemplateName = null,
            Guid? roleTemplateGuid = null)
        {
            EnsureBranchCountersSynced(user, branchModel);
            var content = new MultipartFormDataContent();
            var addedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            Action<string, string> addPart = (key, val) =>
            {
                if (!string.IsNullOrEmpty(key) && addedKeys.Add(key))
                {
                    content.Add(new StringContent(val), key);
                }
            };

            Action<string, string> addField = (name, value) => {
                // Canonical model binding prefix
                addPart($"Profile.User.{name}", value);
                addPart(name, value);
                addPart($"user.{name}", value);

                if (name.Equals("user_code", StringComparison.OrdinalIgnoreCase))
                {
                    addPart("Profile.User.usercode", value);
                    addPart("usercode", value);
                }
                else if (name.Equals("bh_code", StringComparison.OrdinalIgnoreCase) || name.Equals("bhcode", StringComparison.OrdinalIgnoreCase))
                {
                    addPart("Profile.User.bhcode", value);
                    addPart("Profile.User.bh_code", value);
                    addPart("bhcode", value);
                    addPart("bh_code", value);
                }
                else if (name.Equals("cnt_code", StringComparison.OrdinalIgnoreCase) || name.Equals("cntcode", StringComparison.OrdinalIgnoreCase))
                {
                    addPart("Profile.User.cntcode", value);
                    addPart("Profile.User.cnt_code", value);
                    addPart("cntcode", value);
                    addPart("cnt_code", value);
                }
            };

            foreach (var prop in user.GetType().GetProperties())
            {
                if (prop.Name == "user_image" || prop.Name == "signature_image") continue;

                // Skip calculated / redundant aliases on user_master that duplicate primary properties
                if (prop.Name == "usercode" || prop.Name == "userCode" || prop.Name == "bhcode" || prop.Name == "cntcode") continue;

                // When updating, do not send password if it is empty, null, whitespace, dummy placeholder, or already a bcrypt hash
                if (isUpdate && prop.Name.Equals("password", StringComparison.OrdinalIgnoreCase))
                {
                    var pwdVal = prop.GetValue(user)?.ToString();
                    if (string.IsNullOrWhiteSpace(pwdVal) || 
                        pwdVal == "********" || 
                        pwdVal == "••••••••" || 
                        pwdVal.StartsWith("$2a$") || 
                        pwdVal.StartsWith("$2b$") || 
                        pwdVal.StartsWith("$2y$"))
                    {
                        continue;
                    }
                }
                
                var value = prop.GetValue(user);
                if (value != null)
                {
                    string stringValue = value is DateTime dt ? dt.ToString("yyyy-MM-ddTHH:mm:ss") 
                                       : value is bool b ? b.ToString().ToLower()
                                       : value.ToString() ?? "";
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

            // Role IDs
            if (roleIds != null && roleIds.Count > 0)
            {
                // When <= 350 rights are selected, send both canonical Profile.RoleIds[{i}] and roleIds[{i}].
                // When > 350 rights are selected, send canonical Profile.RoleIds[{i}] so the total form
                // value count strictly stays well below ASP.NET Core's 1024 form value limit.
                bool includeUnprefixed = roleIds.Count <= 350;
                for (int i = 0; i < roleIds.Count; i++)
                {
                    string idStr = roleIds[i].ToString();
                    addPart($"Profile.RoleIds[{i}]", idStr);
                    if (includeUnprefixed)
                    {
                        addPart($"roleIds[{i}]", idStr);
                    }
                }
                
                // Also provide comma-delimited list for any backend binders expecting a string
                string csv = string.Join(",", roleIds);
                addPart("Profile.RoleIds", csv);
                addPart("role_ids", csv);
                addPart("roleIds", csv);
            }

            // Role Template
            if (!string.IsNullOrEmpty(roleTemplateName))
            {
                addPart("Profile.RoleTemplateName", roleTemplateName);
                addPart("roleTemplateName", roleTemplateName);
            }
            if (roleTemplateGuid.HasValue)
            {
                addPart("Profile.RoleTemplateGuid", roleTemplateGuid.Value.ToString());
                addPart("roleTemplateGuid", roleTemplateGuid.Value.ToString());
            }

            // Branches
            if (branchModel != null && branchModel.Count > 0)
            {
                for (int i = 0; i < branchModel.Count; i++)
                {
                    var branch = branchModel[i];
                    if (branch.bhcode.HasValue)
                    {
                        addPart($"Profile.Branches[{i}].bhcode", branch.bhcode.Value.ToString());
                        addPart($"branches[{i}].bhcode", branch.bhcode.Value.ToString());
                    }
                    if (branch.cntcode.HasValue)
                    {
                        addPart($"Profile.Branches[{i}].cntcode", branch.cntcode.Value.ToString());
                        addPart($"branches[{i}].cntcode", branch.cntcode.Value.ToString());
                    }
                    if (branch.user_code.HasValue)
                    {
                        addPart($"Profile.Branches[{i}].user_code", branch.user_code.Value.ToString());
                        addPart($"branches[{i}].user_code", branch.user_code.Value.ToString());
                    }
                    if (!string.IsNullOrEmpty(branch.tenant_code))
                    {
                        addPart($"Profile.Branches[{i}].tenant_code", branch.tenant_code);
                        addPart($"branches[{i}].tenant_code", branch.tenant_code);
                    }
                    addPart($"Profile.Branches[{i}].deleted", branch.deleted.ToString().ToLower());
                    addPart($"branches[{i}].deleted", branch.deleted.ToString().ToLower());
                }
            }

            // Departments (Groups)
            if (departmentModel != null && departmentModel.Count > 0)
            {
                for (int i = 0; i < departmentModel.Count; i++)
                {
                    var dept = departmentModel[i];
                    if (dept.dcode.HasValue)
                    {
                        addPart($"Profile.Departments[{i}].gcode", dept.dcode.Value.ToString()); // Map dcode -> gcode
                        addPart($"departments[{i}].gcode", dept.dcode.Value.ToString());
                    }
                    if (dept.user_code.HasValue)
                    {
                        addPart($"Profile.Departments[{i}].user_code", dept.user_code.Value.ToString());
                        addPart($"departments[{i}].user_code", dept.user_code.Value.ToString());
                    }
                    if (!string.IsNullOrEmpty(dept.tenant_code))
                    {
                        addPart($"Profile.Departments[{i}].tenant_code", dept.tenant_code);
                        addPart($"departments[{i}].tenant_code", dept.tenant_code);
                    }
                    addPart($"Profile.Departments[{i}].deleted", dept.deleted.ToString().ToLower());
                    addPart($"departments[{i}].deleted", dept.deleted.ToString().ToLower());
                }
            }

            // Files
            if (userImageBytes != null && userImageBytes.Length > 0 && !string.IsNullOrEmpty(userImageName))
            {
                var fileContent = new ByteArrayContent(userImageBytes);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(GetMimeType(userImageName));
                
                string fileKey = isUpdate ? "userImageFile" : "customerImageFile";
                content.Add(fileContent, fileKey, userImageName);
            }
            if (signatureImageBytes != null && signatureImageBytes.Length > 0 && !string.IsNullOrEmpty(signatureImageName))
            {
                var fileContent = new ByteArrayContent(signatureImageBytes);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(GetMimeType(signatureImageName));
                content.Add(fileContent, "signatureImageFile", signatureImageName);
            }
            Console.WriteLine($"=== TOTAL FORM VALUES COUNT: {addedKeys.Count} (limit is 1024) ===");
            Console.WriteLine("=== EXACT MULTIPART FORM DATA PAYLOAD ===");
            foreach (var p in content)
            {
                if (p.Headers.ContentDisposition != null)
                {
                    var name = p.Headers.ContentDisposition.Name?.Trim('"');
                    if (p is StringContent stringContent)
                    {
                        var val = stringContent.ReadAsStringAsync().Result;
                        Console.WriteLine($"{name}: {val}");
                    }
                    else if (p is ByteArrayContent byteContent)
                    {
                        var fileName = p.Headers.ContentDisposition.FileName?.Trim('"');
                        Console.WriteLine($"{name}: [FILE] {fileName} ({byteContent.Headers.ContentLength} bytes)");
                    }
                }
            }
            Console.WriteLine("=========================================");

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
