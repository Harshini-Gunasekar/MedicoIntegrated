using LabCare.Models;
using LIMS_Backend.Model;
using Medico_Backend.Model;
using System.Net.Http.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Net.Http;
using System.Linq;
using SharedComponents.Rcl.Services;

namespace LabCare.Services
{
    public class TestService
    {
        private readonly HttpClient _http;
        private readonly TenantSessionState _session;

        public TestService(HttpClient http, TenantSessionState session)
        {
            _http = http;
            _session = session;
        }

        private void ConfigureHeaders()
        {
            if (!string.IsNullOrEmpty(_session.AuthToken))
                _http.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.AuthToken);

            if (!string.IsNullOrEmpty(_session.TenantCode))
            {
                if (_http.DefaultRequestHeaders.Contains("tenant_code"))
                    _http.DefaultRequestHeaders.Remove("tenant_code");
                _http.DefaultRequestHeaders.Add("tenant_code", _session.TenantCode);
            }
        }

        private static JsonSerializerSettings JsonSettings => new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Include,
            DateFormatString = "yyyy-MM-ddTHH:mm:ss.fffK",
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        private async Task EnsureSuccessOrThrowAsync(HttpResponseMessage response, string operation)
        {
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new Exception($"{operation} failed ({(int)response.StatusCode}): {body}");
            }
        }

        private void SanitizeForSave(TestInsertDto dto)
        {
            if (dto == null) return;
            if (dto.ResultRows == null) return;
            foreach (var row in dto.ResultRows)
            {
                if (row.ResultMaster != null)
                {
                    row.ResultMaster.deleted ??= false;
                    row.ResultMaster.printinseparatepage ??= false;
                    row.ResultMaster.iscalculated ??= false;
                    row.ResultMaster.isentered ??= false;
                    row.ResultMaster.sendsms ??= false;
                    row.ResultMaster.is_escalation ??= false;
                }

                if (row.ResultProperties != null)
                {
                    row.ResultProperties.simplenormalvalues ??= false;
                    row.ResultProperties.detailednormalvalues ??= false;
                    row.ResultProperties.showagedbased ??= false;
                    row.ResultProperties.printconclusioninreport ??= false;
                    row.ResultProperties.printconclusioninbottom ??= false;
                    row.ResultProperties.showalertonhigherlower ??= false;
                    row.ResultProperties.isaddresult ??= false;
                    row.ResultProperties.printunitsinnormalvalues ??= false;
                    row.ResultProperties.printnormalvaluesatbottom ??= false;
                    row.ResultProperties.printspecialfieldsatrightside ??= false;
                    row.ResultProperties.groupvaluesbysex ??= false;
                    row.ResultProperties.groupvaluesbyspecialfield ??= false;
                    row.ResultProperties.printfixedtextconclusioninreport ??= false;
                    row.ResultProperties.printresultonly ??= false;
                    row.ResultProperties.isgraph ??= false;
                    row.ResultProperties.isabnormal ??= false;
                    row.ResultProperties.usedefault ??= false;
                    row.ResultProperties.istestimage ??= false;
                }
            }
        }

        private static string? ToFormString(object? value)
        {
            if (value == null) return null;
            return value switch
            {
                bool b              => b ? "true" : "false",
                DateTime dt         => dt.ToString("yyyy-MM-ddTHH:mm:ss"),
                DateTimeOffset dto  => dto.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                Guid g              => g.ToString(),
                _                   => value.ToString()
            };
        }

        private static void AddField(MultipartFormDataContent form, string key, object? value)
        {
            if (value == null) return;
            if (value is Guid g && g == Guid.Empty) return;
            var str = ToFormString(value);
            if (str == null) return;
            form.Add(new StringContent(str), key);
        }

        private static MultipartFormDataContent BuildMultipartContent(
            TestInsertDto dto,
            byte[]? testImageBytes = null,
            string? testImageName = null)
        {
            var form = new MultipartFormDataContent();

            AddField(form, "tcode", dto.tcode);
            AddField(form, "totalResults", dto.totalResults);

            var tm = dto.TestMaster;
            if (tm != null)
            {
                foreach (var prop in tm.GetType().GetProperties())
                {
                    if (prop.Name == "test_type_name") continue;
                    AddField(form, $"TestMaster.{prop.Name}", prop.GetValue(tm));
                }
            }

            if (dto.ResultRows != null)
            {
                for (int i = 0; i < dto.ResultRows.Count; i++)
                {
                    var row = dto.ResultRows[i];
                    string rowPrefix = $"ResultRows[{i}]";

                    if (row.ResultMaster != null)
                    {
                        foreach (var prop in row.ResultMaster.GetType().GetProperties())
                            AddField(form, $"{rowPrefix}.ResultMaster.{prop.Name}", prop.GetValue(row.ResultMaster));
                    }

                    if (row.ResultProperties != null)
                    {
                        foreach (var prop in row.ResultProperties.GetType().GetProperties())
                            AddField(form, $"{rowPrefix}.ResultProperties.{prop.Name}", prop.GetValue(row.ResultProperties));
                    }

                    if (row.DetailedNormalValues != null)
                    {
                        for (int j = 0; j < row.DetailedNormalValues.Count; j++)
                        {
                            var dnv = row.DetailedNormalValues[j];
                            foreach (var prop in dnv.GetType().GetProperties())
                                AddField(form, $"{rowPrefix}.DetailedNormalValues[{j}].{prop.Name}", prop.GetValue(dnv));
                        }
                    }

                    if (row.TextNormalValues != null)
                    {
                        for (int j = 0; j < row.TextNormalValues.Count; j++)
                        {
                            var tnv = row.TextNormalValues[j];
                            foreach (var prop in tnv.GetType().GetProperties())
                                AddField(form, $"{rowPrefix}.TextNormalValues[{j}].{prop.Name}", prop.GetValue(tnv));
                        }
                    }

                    if (row.CalculatedFormulas != null)
                    {
                        for (int j = 0; j < row.CalculatedFormulas.Count; j++)
                        {
                            var cf = row.CalculatedFormulas[j];
                            foreach (var prop in cf.GetType().GetProperties())
                                AddField(form, $"{rowPrefix}.CalculatedFormulas[{j}].{prop.Name}", prop.GetValue(cf));
                        }
                    }

                    if (row.testImageBytes != null && row.testImageBytes.Length > 0)
                    {
                        string imgName = row.testImageName ?? "row_image.png";
                        string mime = imgName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                      imgName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
                                      ? "image/jpeg" : "image/png";
                        var fileContent = new ByteArrayContent(row.testImageBytes);
                        fileContent.Headers.ContentType =
                            new System.Net.Http.Headers.MediaTypeHeaderValue(mime);
                        form.Add(fileContent, "testImageFiles", imgName);
                    }
                }
            }

            if (testImageBytes != null && testImageBytes.Length > 0)
            {
                string imgName = testImageName ?? "test_image.png";
                string mime = imgName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                              imgName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
                              ? "image/jpeg" : "image/png";
                var fileContent = new ByteArrayContent(testImageBytes);
                fileContent.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue(mime);
                form.Add(fileContent, "testImageFile", imgName);
            }

            return form;
        }

        private void InitialiseResultRows(TestInsertDto dto, bool isInsert)
        {
            if (dto.ResultRows == null) return;

            foreach (var row in dto.ResultRows)
            {
                var rm = row.ResultMaster;
                if (rm == null) continue;
                rm.tcode = dto.TestMaster.tcode;
                rm.tenant_code ??= _session.TenantCode;
                rm.ibsdate = DateTime.Now;
                rm.entereddate ??= DateTime.Now;
                rm.usercode ??= 1;
                rm.computercode ??= 1;

                if (rm.testresultid == Guid.Empty) rm.testresultid = Guid.NewGuid();
                if (rm.trguid == null || rm.trguid == Guid.Empty) rm.trguid = Guid.NewGuid();

                if (!string.IsNullOrWhiteSpace(rm.cellcontent))
                    rm.col2 = rm.cellcontent;
                else if (!string.IsNullOrWhiteSpace(rm.col2))
                    rm.cellcontent = rm.col2;

                if (row.ResultProperties != null && !string.IsNullOrWhiteSpace(row.ResultProperties.resultvaluetype))
                {
                    rm.resulttype = row.ResultProperties.resultvaluetype switch
                    {
                        "Number"           => "N",
                        "Numeric"          => "N",
                        "Text"             => "T",
                        "Text & Number"    => "B",
                        "Fixed Type"       => "F",
                        "FixedText"        => "F",
                        "Calculated Value" => "C",
                        "Culture"          => "L",
                        "Title"            => "H",
                        "SubTitle"         => "S",
                        "N"  => "N",
                        "T"  => "T",
                        "B"  => "B",
                        "TN" => "B",
                        "F"  => "F",
                        "C"  => "C",
                        "L"  => "L",
                        "H"  => "H",
                        "S"  => "S",
                        _    => "N"
                    };
                }
                else if (string.IsNullOrWhiteSpace(rm.resulttype))
                {
                    rm.resulttype = "N";
                }

                var rp = row.ResultProperties;
                if (rp != null)
                {
                    rp.tenant_code ??= _session.TenantCode;
                    rp.testresultid = rm.testresultid;
                    if (rp.trpid == Guid.Empty) rp.trpid = Guid.NewGuid();
                    rp.entereddate ??= DateTime.Now;
                    rp.mccode ??= 0;
                    rp.scode ??= 0;
                    rp.rtmcode ??= 0;
                    rp.defaultunitscode ??= 0;
                    rp.decimalvalue ??= 0;
                    rp.fromnormalvalue ??= 0;
                    rp.tonormalvalue ??= 0;
                    rp.istestimage ??= false;
                }

                if (row.CalculatedFormulas != null)
                {
                    foreach (var cf in row.CalculatedFormulas)
                    {
                        cf.tenant_code ??= _session.TenantCode;
                        cf.testresultid ??= rm.testresultid;
                        if (cf.trcfid == Guid.Empty) cf.trcfid = Guid.NewGuid();
                        cf.mccode ??= 0;
                        cf.scode ??= 0;
                        cf.entereddate ??= DateTime.Now;
                    }
                }

                if (row.DetailedNormalValues != null)
                {
                    foreach (var dnv in row.DetailedNormalValues)
                    {
                        dnv.tenant_code ??= _session.TenantCode;
                        dnv.testresultid ??= rm.testresultid;
                        if (dnv.trdnid == Guid.Empty) dnv.trdnid = Guid.NewGuid();
                        dnv.mccode ??= 0;
                        dnv.scode ??= 0;
                        dnv.entereddate ??= DateTime.Now;
                    }
                }

                if (row.TextNormalValues != null)
                {
                    foreach (var tnv in row.TextNormalValues)
                    {
                        tnv.tenant_code ??= _session.TenantCode;
                        tnv.testresultid ??= rm.testresultid;
                        if (tnv.trtid == Guid.Empty) tnv.trtid = Guid.NewGuid();
                        tnv.mccode ??= 0;
                        tnv.scode ??= 0;
                        tnv.entereddate ??= DateTime.Now;
                    }
                }
            }
        }

        public async Task<List<test_master>> GetAllTestsAsync(string? search = null)
        {
            ConfigureHeaders();
            var url = "api/Test/get?t=" + DateTime.Now.Ticks;
            if (!string.IsNullOrEmpty(search)) url += $"&search={Uri.EscapeDataString(search)}";
            
            try
            {
                var response = await _http.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    return new();
                }
                var rawJson = await response.Content.ReadAsStringAsync();
                var deserialized = JsonConvert.DeserializeObject<List<test_master>>(rawJson, JsonSettings) ?? new();
                return deserialized;
            }
            catch
            {
                return new();
            }
        }

        public async Task<bool> InsertTestAsync(
            TestInsertDto dto,
            byte[]? testImageBytes = null,
            string? testImageName = null)
        {
            ConfigureHeaders();

            dto.TestMaster.tenant_code ??= _session.TenantCode;
            dto.TestMaster.computercode ??= 1;
            dto.TestMaster.usercode ??= 1;
            dto.TestMaster.ibsdate ??= DateTime.Now;
            dto.TestMaster.entereddate ??= DateTime.Now;
            if (string.IsNullOrEmpty(dto.TestMaster.qty))
                dto.TestMaster.qty = "1";

            InitialiseResultRows(dto, isInsert: true);
            SanitizeForSave(dto);

            dto.tcode = dto.TestMaster.tcode;
            dto.totalResults = dto.ResultRows?.Count ?? 0;

            var formContent = BuildMultipartContent(dto, testImageBytes, testImageName);

            var response = await _http.PostAsync("api/Test/insert", formContent);
            await EnsureSuccessOrThrowAsync(response, "Insert Test");

            var body = await response.Content.ReadAsStringAsync();
            ExtractTcodeFromResponse(body, dto);

            if (dto.TestMaster.tcode == 0 && !string.IsNullOrWhiteSpace(dto.TestMaster.name))
            {
                await Task.Delay(1000);
                var allTests = await GetAllTestsAsync();
                var searchName = dto.TestMaster.name.Trim();
                var matched = allTests
                    .Where(t => t.name != null && t.name.Trim().Equals(searchName, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(t => t.tcode)
                    .FirstOrDefault();

                if (matched != null)
                {
                    dto.TestMaster.tcode = matched.tcode;
                }
            }

            return true;
        }

        public async Task<bool> UpdateTestAsync(
            TestInsertDto dto,
            byte[]? testImageBytes = null,
            string? testImageName = null)
        {
            ConfigureHeaders();
            if (dto.TestMaster == null) throw new Exception("Update failed: TestMaster is null");

            dto.TestMaster.tenant_code ??= _session.TenantCode;
            dto.TestMaster.computercode ??= 1;
            dto.TestMaster.usercode ??= 1;
            dto.TestMaster.ibsdate = DateTime.Now;
            if (string.IsNullOrEmpty(dto.TestMaster.qty))
                dto.TestMaster.qty = "1";

            InitialiseResultRows(dto, isInsert: false);
            SanitizeForSave(dto);

            dto.tcode = dto.TestMaster.tcode;
            dto.totalResults = dto.ResultRows?.Count ?? 0;

            var formContent = BuildMultipartContent(dto, testImageBytes, testImageName);

            var response = await _http.PostAsync("api/Test/update", formContent);
            await EnsureSuccessOrThrowAsync(response, "Update Test");
            return true;
        }

        private void ExtractTcodeFromResponse(string body, TestInsertDto dto)
        {
            if (string.IsNullOrWhiteSpace(body)) return;

            if (long.TryParse(body.Trim(), out long direct) && direct > 0)
            {
                dto.TestMaster.tcode = direct;
                return;
            }

            try
            {
                var obj = JsonConvert.DeserializeObject<JObject>(body, JsonSettings);
                if (obj == null) return;

                foreach (var key in new[] { "tcode", "TCode", "tCode" })
                {
                    var prop = obj.Property(key, StringComparison.OrdinalIgnoreCase);
                    if (prop != null && long.TryParse(prop.Value.ToString(), out long parsed) && parsed > 0)
                    {
                        dto.TestMaster.tcode = parsed;
                        return;
                    }
                }
            }
            catch { }
        }

        public async Task<bool> SoftDeleteTestAsync(long tcode)
        {
            ConfigureHeaders();
            var response = await _http.DeleteAsync($"api/Test/softdelete?tcode={tcode}");
            if (response.IsSuccessStatusCode) return true;

            try
            {
                var postResponse = await _http.PostAsJsonAsync($"api/Test/softdelete?tcode={tcode}", new { });
                if (postResponse.IsSuccessStatusCode) return true;
            }
            catch {}

            try
            {
                var getResponse = await _http.GetAsync($"api/Test/softdelete?tcode={tcode}");
                if (getResponse.IsSuccessStatusCode) return true;
            }
            catch {}

            return false;
        }

        public async Task<bool> HardDeleteTestAsync(long tcode)
        {
            ConfigureHeaders();
            var response = await _http.DeleteAsync($"api/Test/softdelete?tcode={tcode}");
            return response.IsSuccessStatusCode;
        }

        public async Task<TestInsertDto?> GetTestResultAsync(long tcode)
        {
            ConfigureHeaders();
            try
            {
                var tenantCode = _session.TenantCode ?? "";
                var url = $"api/Test/Result/get?tcode={tcode}&tenant_code={tenantCode}&t={DateTime.Now.Ticks}";

                var response = await _http.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    // Fallback to api/Test/get?tcode= if Result/get is not defined on the new controller
                    url = $"api/Test/get?tcode={tcode}&tenant_code={tenantCode}&t={DateTime.Now.Ticks}";
                    response = await _http.GetAsync(url);
                }

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var rawJson = await response.Content.ReadAsStringAsync();
                var fetchDto = JsonConvert.DeserializeObject<TestFetchDto>(rawJson, JsonSettings);

                if (fetchDto != null && fetchDto.Results != null && fetchDto.Results.Count > 0)
                {
                    var resultDto = new TestInsertDto
                    {
                        TestMaster = fetchDto.TestMaster ?? new test_master { tcode = fetchDto.TCode },
                        ResultRows = fetchDto.Results
                    };

                    NormaliseResultRows(resultDto);
                    return resultDto;
                }

                var outer = JsonConvert.DeserializeObject<JObject>(rawJson, JsonSettings);
                if (outer == null) return null;

                JArray? resultsArray = null;
                foreach (var key in new[] { "results", "Results", "resultRows", "ResultRows" })
                {
                    if (outer[key] is JArray arr) { resultsArray = arr; break; }
                }

                if (resultsArray == null || !resultsArray.Any()) return null;

                var rows = resultsArray.ToObject<List<TestResultRowDto>>(JsonSerializer.Create(JsonSettings));
                var master = outer["testMaster"]?.ToObject<test_master>(JsonSerializer.Create(JsonSettings))
                          ?? outer["TestMaster"]?.ToObject<test_master>(JsonSerializer.Create(JsonSettings))
                          ?? new test_master { tcode = tcode, tenant_code = _session.TenantCode };

                var dto = new TestInsertDto { TestMaster = master, ResultRows = rows ?? new() };
                NormaliseResultRows(dto);
                return dto;
            }
            catch
            {
                return null;
            }
        }

        private void NormaliseResultRows(TestInsertDto dto)
        {
            if (dto.ResultRows == null) return;
            foreach (var row in dto.ResultRows)
            {
                if (row.ResultMaster == null) continue;
                row.ResultMaster.tcode = dto.TestMaster.tcode;

                if (string.IsNullOrWhiteSpace(row.ResultMaster.cellcontent))
                    row.ResultMaster.cellcontent = row.ResultMaster.col2;

                row.ResultProperties ??= new test_result_properties
                {
                    trpid = Guid.NewGuid(),
                    testresultid = row.ResultMaster.testresultid,
                    resultvaluetype = "Number"
                };

                row.ResultProperties.resultvaluetype = row.ResultProperties.resultvaluetype switch
                {
                    "N"         => "Number",
                    "B"         => "Text & Number",
                    "TN"        => "Text & Number",
                    "T"         => "Text",
                    "C"         => "Calculated Value",
                    "F"         => "Fixed Type",
                    "L"         => "Culture",
                    "CU"        => "Culture",
                    "H"         => "Title",
                    "S"         => "SubTitle",
                    "FixedText" => "Fixed Type",
                    "Numeric"   => "Number",
                    _           => row.ResultProperties.resultvaluetype ?? "Number"
                };

                row.CalculatedFormulas ??= new();
                row.DetailedNormalValues ??= new();
                row.TextNormalValues ??= new();
            }
        }

        public async Task<List<test_type_master>> GetTestTypesAsync()
        {
            ConfigureHeaders();
            try { return await _http.GetFromJsonAsync<List<test_type_master>>("TestType/get") ?? new(); }
            catch { return new(); }
        }

        public async Task<bool> InsertTestTypeAsync(test_type_master testType)
        {
            ConfigureHeaders();
            var response = await _http.PostAsJsonAsync("TestType/insert", testType);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateTestTypeAsync(test_type_master testType)
        {
            ConfigureHeaders();
            var response = await _http.PostAsJsonAsync("TestType/update", testType);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteTestTypeAsync(long ttid)
        {
            ConfigureHeaders();
            var response = await _http.DeleteAsync($"TestType/delete?ttid={ttid}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> SoftDeleteTestTypeAsync(long ttid)
        {
            ConfigureHeaders();
            var response = await _http.DeleteAsync($"TestType/softdelete?ttid={ttid}");
            if (response.IsSuccessStatusCode) return true;

            try
            {
                var postResponse = await _http.PostAsJsonAsync($"TestType/softdelete?ttid={ttid}", new { });
                if (postResponse.IsSuccessStatusCode) return true;
            }
            catch {}

            try
            {
                var getResponse = await _http.GetAsync($"TestType/softdelete?ttid={ttid}");
                if (getResponse.IsSuccessStatusCode) return true;
            }
            catch {}

            return false;
        }

        public async Task<List<SampleModel>> GetSpecimensAsync()
        {
            ConfigureHeaders();
            try
            {
                var response = await _http.GetAsync("Sample/get");
                if (!response.IsSuccessStatusCode) return new();
                var rawJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<SampleModel>>(rawJson, JsonSettings) ?? new();
            }
            catch { return new(); }
        }

        public async Task<List<UomMaster>> GetUnitsAsync()
        {
            ConfigureHeaders();
            try
            {
                var response = await _http.GetAsync("Uom/get");
                if (!response.IsSuccessStatusCode) return new();
                var rawJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<UomMaster>>(rawJson, JsonSettings) ?? new();
            }
            catch { return new(); }
        }
        
        public async Task<List<MachineMasterModel>> GetMachinesAsync()
        {
            ConfigureHeaders();
            try
            {
                var response = await _http.GetAsync("api/MachineMaster/get");
                if (!response.IsSuccessStatusCode) return new();
                var rawJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<MachineMasterModel>>(rawJson, JsonSettings) ?? new();
            }
            catch { return new(); }
        }

        public async Task<bool> InsertMachineAsync(MachineMasterModel machine)
        {
            ConfigureHeaders();
            var response = await _http.PostAsJsonAsync("api/MachineMaster/insert", machine);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateMachineAsync(MachineMasterModel machine)
        {
            ConfigureHeaders();
            var response = await _http.PostAsJsonAsync("api/MachineMaster/update", machine);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> SoftDeleteMachineAsync(int mccode)
        {
            ConfigureHeaders();
            var response = await _http.DeleteAsync($"api/MachineMaster/delete?mccode={mccode}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteMachineAsync(int mccode)
        {
            ConfigureHeaders();
            var response = await _http.DeleteAsync($"api/MachineMaster/delete?mccode={mccode}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<ReportMethodModel>> GetReportMethodsAsync()
        {
            ConfigureHeaders();
            try
            {
                var response = await _http.GetAsync("ReportMethod/get");
                if (!response.IsSuccessStatusCode) return new();
                var rawJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<ReportMethodModel>>(rawJson, JsonSettings) ?? new();
            }
            catch { return new(); }
        }

        public async Task<List<ReportingModel>> GetReportingsAsync()
        {
            ConfigureHeaders();
            try
            {
                var response = await _http.GetAsync("Reporting/get");
                if (!response.IsSuccessStatusCode) return new();
                var rawJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<ReportingModel>>(rawJson, JsonSettings) ?? new();
            }
            catch { return new(); }
        }

        public async Task<long> InsertReportingAsync(ReportingModel model)
        {
            ConfigureHeaders();
            model.tenant_code ??= _session.TenantCode;
            model.entereddate ??= DateTime.Now;
            model.ibsdate = DateTime.Now;
            model.orderno = 1;
            model.description ??= "";
            model.shortname ??= "";
            model.signatureimage ??= Array.Empty<byte>();

            var json = JsonConvert.SerializeObject(model, Formatting.None, JsonSettings);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await _http.PostAsync("Reporting/insert", content);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to insert reporting: {body}");
            }
            
            var respBody = await response.Content.ReadAsStringAsync();
            if (long.TryParse(respBody.Trim(), out long direct) && direct > 0)
            {
                return direct;
            }
            try
            {
                var obj = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(respBody);
                if (obj != null)
                {
                    foreach (var key in new[] { "recode", "Recode", "reCode" })
                    {
                        var prop = obj.Property(key, StringComparison.OrdinalIgnoreCase);
                        if (prop != null && long.TryParse(prop.Value.ToString(), out long parsed) && parsed > 0)
                        {
                            return parsed;
                        }
                    }
                }
            }
            catch { }

            var all = await GetReportingsAsync();
            var matched = all.FirstOrDefault(r => r.name != null && r.name.Equals(model.name, StringComparison.OrdinalIgnoreCase));
            return matched?.recode ?? 0;
        }

        public async Task<List<ReportMethodModel>> GetReportMethodsCultureAsync()
        {
            ConfigureHeaders();
            try
            {
                var response = await _http.GetAsync("ReportMethod/get-culture");
                if (!response.IsSuccessStatusCode) return new();
                var rawJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<ReportMethodModel>>(rawJson, JsonSettings) ?? new();
            }
            catch { return new(); }
        }

        public async Task<bool> InsertReportMethodAsync(ReportMethodModel model)
        {
            ConfigureHeaders();
            model.tenant_code ??= _session.TenantCode;
            if (model.entereddate == default) model.entereddate = DateTime.Now;
            model.ibsdate = DateTime.Now;
            var response = await _http.PostAsJsonAsync("ReportMethod/update", model);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateReportMethodAsync(ReportMethodModel model)
        {
            ConfigureHeaders();
            model.tenant_code ??= _session.TenantCode;
            model.ibsdate = DateTime.Now;
            var response = await _http.PostAsJsonAsync("ReportMethod/update", model);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> SoftDeleteReportMethodAsync(long rtmcode)
        {
            ConfigureHeaders();
            var response = await _http.GetAsync($"ReportMethod/softdelete?rtmcode={rtmcode}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteReportMethodAsync(long rtmcode)
        {
            ConfigureHeaders();
            var response = await _http.DeleteAsync($"ReportMethod/delete?rtmcode={rtmcode}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> InsertUnitAsync(UomMaster model)
        {
            ConfigureHeaders();
            model.tenant_code ??= _session.TenantCode;
            var response = await _http.PostAsJsonAsync("Uom/insert", model);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateUnitAsync(UomMaster model)
        {
            ConfigureHeaders();
            model.tenant_code ??= _session.TenantCode;
            var response = await _http.PostAsJsonAsync("Uom/update", model);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> SoftDeleteUnitAsync(long ucode)
        {
            ConfigureHeaders();
            var response = await _http.GetAsync($"Uom/softdelete?ucode={ucode}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteUnitAsync(long ucode)
        {
            ConfigureHeaders();
            var response = await _http.DeleteAsync($"Uom/delete?ucode={ucode}");
            return response.IsSuccessStatusCode;
        }
    }
}
