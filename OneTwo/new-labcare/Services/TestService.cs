using LabCare.Models;
using LIMS_Backend.Model;
using System.Net.Http.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Net.Http;
using System.Linq;

namespace LabCare.Services
{
    /// <summary>
    /// Service for managing laboratory tests and results.
    ///
    /// KEY DESIGN DECISIONS (fixes applied):
    ///  1. All JSON serialisation uses lowercase JsonProperty names (matching the API).
    ///     We NO LONGER use CamelCasePropertyNamesContractResolver because it conflicts
    ///     with explicit [JsonProperty] attributes and produces double-mapped / wrong keys.
    ///  2. INSERT  → POST Test/insert   payload shape: { tcode, totalResults, testMaster, results[] }
    ///  3. UPDATE  → POST Test/update   payload shape: same as INSERT (flat lowercase object)
    ///  4. GET     → GET  Test/Result/get  response shape: { tcode, totalResults, testMaster, results[] }
    ///     TestFetchDto now has [JsonProperty("testMaster")] and [JsonProperty("results")] to match.
    ///  5. trcode on each resultMaster is assigned from the fetched value; never zeroed-out.
    /// </summary>
    public class TestService
    {
        private readonly HttpClient _http;
        private readonly TenantSessionState _session;

        public TestService(HttpClient http, TenantSessionState session)
        {
            _http = http;
            _session = session;
        }

        // ─── Helpers ────────────────────────────────────────────────────────────

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

        /// <summary>
        /// Single, consistent serialiser settings.
        /// Uses the [JsonProperty] attributes on the model classes (all lowercase).
        /// Do NOT add CamelCasePropertyNamesContractResolver here – it overrides
        /// [JsonProperty] and produces wrong key names.
        /// </summary>
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

        // ─── Boolean sanitiser ──────────────────────────────────────────────────

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

        // ─── Payload builders ───────────────────────────────────────────────────

        /// <summary>
        /// Converts a value to its form-data string representation.
        /// Booleans are lowercased ("true"/"false"), DateTimeOffset uses ISO-8601,
        /// Guid uses standard format, all others use ToString().
        /// </summary>
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

        /// <summary>
        /// Adds a single form field. Null values and empty Guids are skipped.
        /// </summary>
        private static void AddField(MultipartFormDataContent form, string key, object? value)
        {
            if (value == null) return;
            if (value is Guid g && g == Guid.Empty) return;
            var str = ToFormString(value);
            if (str == null) return;
            form.Add(new StringContent(str), key);
        }

        /// <summary>
        /// Builds a multipart/form-data payload from <see cref="TestInsertDto"/> using
        /// the flat-key naming convention expected by the backend:
        ///   TestMaster.{field}
        ///   ResultRows[i].ResultMaster.{field}
        ///   ResultRows[i].ResultProperties.{field}
        ///   ResultRows[i].DetailedNormalValues[j].{field}
        ///   ResultRows[i].TextNormalValues[j].{field}
        ///   ResultRows[i].CalculatedFormulas[j].{field}
        ///   testImageFile  (optional file part)
        /// </summary>
        private static MultipartFormDataContent BuildMultipartContent(
            TestInsertDto dto,
            byte[]? testImageBytes = null,
            string? testImageName = null)
        {
            var form = new MultipartFormDataContent();

            // ── Top-level scalars ──────────────────────────────────────────
            AddField(form, "tcode", dto.tcode);
            AddField(form, "totalResults", dto.totalResults);

            // ── TestMaster ────────────────────────────────────────────────
            var tm = dto.TestMaster;
            if (tm != null)
            {
                foreach (var prop in tm.GetType().GetProperties())
                {
                    // Skip computed / navigation properties
                    if (prop.Name == "test_type_name") continue;
                    AddField(form, $"TestMaster.{prop.Name}", prop.GetValue(tm));
                }
            }

            // ── ResultRows ────────────────────────────────────────────────
            if (dto.ResultRows != null)
            {
                for (int i = 0; i < dto.ResultRows.Count; i++)
                {
                    var row = dto.ResultRows[i];
                    string rowPrefix = $"ResultRows[{i}]";

                    // ResultMaster
                    if (row.ResultMaster != null)
                    {
                        foreach (var prop in row.ResultMaster.GetType().GetProperties())
                            AddField(form, $"{rowPrefix}.ResultMaster.{prop.Name}", prop.GetValue(row.ResultMaster));
                    }

                    // ResultProperties
                    if (row.ResultProperties != null)
                    {
                        foreach (var prop in row.ResultProperties.GetType().GetProperties())
                            AddField(form, $"{rowPrefix}.ResultProperties.{prop.Name}", prop.GetValue(row.ResultProperties));
                    }

                    // DetailedNormalValues
                    if (row.DetailedNormalValues != null)
                    {
                        for (int j = 0; j < row.DetailedNormalValues.Count; j++)
                        {
                            var dnv = row.DetailedNormalValues[j];
                            foreach (var prop in dnv.GetType().GetProperties())
                                AddField(form, $"{rowPrefix}.DetailedNormalValues[{j}].{prop.Name}", prop.GetValue(dnv));
                        }
                    }

                    // TextNormalValues
                    if (row.TextNormalValues != null)
                    {
                        for (int j = 0; j < row.TextNormalValues.Count; j++)
                        {
                            var tnv = row.TextNormalValues[j];
                            foreach (var prop in tnv.GetType().GetProperties())
                                AddField(form, $"{rowPrefix}.TextNormalValues[{j}].{prop.Name}", prop.GetValue(tnv));
                        }
                    }

                    // CalculatedFormulas
                    if (row.CalculatedFormulas != null)
                    {
                        for (int j = 0; j < row.CalculatedFormulas.Count; j++)
                        {
                            var cf = row.CalculatedFormulas[j];
                            foreach (var prop in cf.GetType().GetProperties())
                                AddField(form, $"{rowPrefix}.CalculatedFormulas[{j}].{prop.Name}", prop.GetValue(cf));
                        }
                    }

                    // ── Optional row-specific test image file ──────────────────────
                    if (row.testImageBytes != null && row.testImageBytes.Length > 0)
                    {
                        string imgName = row.testImageName ?? "row_image.png";
                        string mime = imgName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                      imgName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
                                      ? "image/jpeg" : "image/png";
                        var fileContent = new ByteArrayContent(row.testImageBytes);
                        fileContent.Headers.ContentType =
                            new System.Net.Http.Headers.MediaTypeHeaderValue(mime);
                        form.Add(fileContent, $"ResultRows[{i}].testImageFile", imgName);
                    }
                }
            }

            // ── Optional test image file ───────────────────────────────────
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

        // ─── Shared sub-record initialiser ──────────────────────────────────────

        private void InitialiseResultRows(TestInsertDto dto, bool isInsert)
        {
            if (dto.ResultRows == null) return;

            foreach (var row in dto.ResultRows)
            {
                // ResultMaster
                var rm = row.ResultMaster;
                rm.tcode = dto.TestMaster.tcode;
                rm.tenant_code ??= _session.TenantCode;
                rm.ibsdate = DateTime.Now;
                rm.entereddate ??= DateTime.Now;
                rm.usercode ??= 1;
                rm.computercode ??= 1;

                if (rm.testresultid == Guid.Empty) rm.testresultid = Guid.NewGuid();
                if (rm.trguid == null || rm.trguid == Guid.Empty) rm.trguid = Guid.NewGuid();

                // Bidirectional sync between cellcontent (UI "Parameter Name" field) and col2 (API field):
                //   • cellcontent has a value → write it to col2 (user input wins)
                //   • cellcontent is empty    → fill it from col2 (fallback for API-loaded rows)
                if (!string.IsNullOrWhiteSpace(rm.cellcontent))
                    rm.col2 = rm.cellcontent;
                else if (!string.IsNullOrWhiteSpace(rm.col2))
                    rm.cellcontent = rm.col2;


                // Map resulttype based on the ResultValueType selection.
                // Handles both UI display names AND API shorthand values.
                if (row.ResultProperties != null && !string.IsNullOrWhiteSpace(row.ResultProperties.resultvaluetype))
                {
                    rm.resulttype = row.ResultProperties.resultvaluetype switch
                    {
                        // ── UI display names ─────────────────────────────────────
                        "Number"           => "N",
                        "Numeric"          => "N",   // loaded from API as "Numeric"
                        "Text"             => "T",
                        "Text & Number"    => "B",
                        "Fixed Type"       => "F",
                        "FixedText"        => "F",   // API variant
                        "Calculated Value" => "C",
                        "Culture"          => "L",
                        "Title"            => "H",
                        "SubTitle"         => "S",
                        // ── API shorthands (already a valid resulttype) ──────────
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

                // ResultProperties
                var rp = row.ResultProperties;
                if (rp != null)
                {
                    rp.tenant_code ??= _session.TenantCode;
                    rp.testresultid = rm.testresultid;   // FK must match
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

                // CalculatedFormulas
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

                // DetailedNormalValues
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

                // TextNormalValues
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

        // ─── Test Master CRUD ───────────────────────────────────────────────────

        public async Task<List<test_master>> GetAllTestsAsync(string? search = null)
        {
            ConfigureHeaders();
            var url = "Test/get?t=" + DateTime.Now.Ticks;
            if (!string.IsNullOrEmpty(search)) url += $"&search={Uri.EscapeDataString(search)}";
            
            var logBuilder = new System.Text.StringBuilder();
            logBuilder.AppendLine($"=== [GetAllTestsAsync] Called at {DateTime.Now} ===");
            logBuilder.AppendLine($"URL: {_http.BaseAddress}{url}");
            logBuilder.AppendLine("Headers:");
            foreach (var h in _http.DefaultRequestHeaders)
            {
                logBuilder.AppendLine($"  {h.Key}: {string.Join(", ", h.Value)}");
            }
            
            try
            {
                var response = await _http.GetAsync(url);
                logBuilder.AppendLine($"Response Status: {response.StatusCode}");
                Console.WriteLine($"[TestService] GetAllTestsAsync HTTP Status: {response.StatusCode}");
                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    logBuilder.AppendLine($"Error Body: {errorBody}");
                    Console.WriteLine($"[TestService] GetAllTestsAsync Error Body: {errorBody}");
                    System.IO.File.AppendAllText("api_logs.txt", logBuilder.ToString() + Environment.NewLine);
                    return new();
                }
                var rawJson = await response.Content.ReadAsStringAsync();
                logBuilder.AppendLine($"Raw JSON Length: {rawJson?.Length ?? 0}");
                logBuilder.AppendLine($"Raw JSON: {rawJson}");
                Console.WriteLine($"[TestService] GetAllTestsAsync Raw JSON Length: {rawJson?.Length ?? 0}");
                
                var deserialized = JsonConvert.DeserializeObject<List<test_master>>(rawJson, JsonSettings) ?? new();
                logBuilder.AppendLine($"Deserialized Count: {deserialized.Count}");
                Console.WriteLine($"[TestService] GetAllTestsAsync Deserialized Count: {deserialized.Count}");
                System.IO.File.AppendAllText("api_logs.txt", logBuilder.ToString() + Environment.NewLine);
                return deserialized;
            }
            catch (Exception ex)
            {
                logBuilder.AppendLine($"Exception: {ex}");
                Console.WriteLine($"[TestService] GetAllTestsAsync Exception: {ex}");
                System.IO.File.AppendAllText("api_logs.txt", logBuilder.ToString() + Environment.NewLine);
                return new();
            }
        }

        public async Task<bool> InsertTestAsync(
            TestInsertDto dto,
            byte[]? testImageBytes = null,
            string? testImageName = null)
        {
            ConfigureHeaders();

            // Master defaults
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

            var form = BuildMultipartContent(dto, testImageBytes, testImageName);

            Console.WriteLine("--- TEST INSERT (multipart form-data) ---");
            Console.WriteLine($"TestMaster.name={dto.TestMaster.name}, ResultRows={dto.totalResults}");
            Console.WriteLine("-----------------------------------------");

            var response = await _http.PostAsync("Test/insert", form);
            await EnsureSuccessOrThrowAsync(response, "Insert Test");

            var body = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[TestService] Insert Response: {body}");

            // Try to extract newly assigned tcode from response
            ExtractTcodeFromResponse(body, dto);

            // If still 0, resolve by name lookup
            if (dto.TestMaster.tcode == 0 && !string.IsNullOrWhiteSpace(dto.TestMaster.name))
            {
                Console.WriteLine($"[TestService] TCode is 0 after insert. Attempting name-based lookup for '{dto.TestMaster.name}'...");
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
                    Console.WriteLine($"[TestService] Resolved TCode via lookup: {matched.tcode}");
                }
                else
                {
                    Console.WriteLine($"[TestService] WARNING: Could not find test by name '{searchName}' after insert. Total tests in list: {allTests.Count}");
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

            var form = BuildMultipartContent(dto, testImageBytes, testImageName);

            Console.WriteLine($"--- TEST UPDATE (tcode={dto.TestMaster.tcode}, multipart form-data) ---");
            Console.WriteLine($"TestMaster.name={dto.TestMaster.name}, ResultRows={dto.totalResults}");
            Console.WriteLine("------------------------------------------------------------------------");

            var response = await _http.PostAsync("Test/update", form);
            await EnsureSuccessOrThrowAsync(response, "Update Test");

            var body = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[TestService] Update SUCCESS: {body}");
            return true;
        }

        /// <summary>
        /// Tries to read the tcode from an insert/update response body.
        /// Handles plain numeric strings and JSON objects.
        /// </summary>
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

                // Try common key names in order
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
            catch { /* ignore parse errors */ }
        }

        public async Task<bool> SoftDeleteTestAsync(long tcode)
        {
            ConfigureHeaders();
            var response = await _http.DeleteAsync($"Test/softdelete?tcode={tcode}");
            if (response.IsSuccessStatusCode) return true;

            try
            {
                var postResponse = await _http.PostAsJsonAsync($"Test/softdelete?tcode={tcode}", new { });
                if (postResponse.IsSuccessStatusCode) return true;
            }
            catch {}

            try
            {
                var getResponse = await _http.GetAsync($"Test/softdelete?tcode={tcode}");
                if (getResponse.IsSuccessStatusCode) return true;
            }
            catch {}

            return false;
        }

        public async Task<bool> HardDeleteTestAsync(long tcode)
        {
            ConfigureHeaders();
            var response = await _http.DeleteAsync($"Test/delete?tcode={tcode}");
            return response.IsSuccessStatusCode;
        }

        // ─── Test Result (fetch) ────────────────────────────────────────────────

        public async Task<TestInsertDto?> GetTestResultAsync(long tcode)
        {
            ConfigureHeaders();
            try
            {
                var tenantCode = _session.TenantCode ?? "";
                var url = $"Test/Result/get?tcode={tcode}&tenant_code={tenantCode}&t={DateTime.Now.Ticks}";
                Console.WriteLine($"[TestService] Fetching Test Result URL: {url}");

                var response = await _http.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    var errBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[TestService] Fetch failed ({response.StatusCode}) for tcode={tcode}: {errBody}");
                    return null;
                }

                var rawJson = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[TestService] Raw JSON for Test {tcode}: {rawJson}");

                // ── Primary path: deserialise via TestFetchDto ──────────────────
                // TestFetchDto now has [JsonProperty("testMaster")] and
                // [JsonProperty("results")] which match the API's lowercase keys.
                var fetchDto = JsonConvert.DeserializeObject<TestFetchDto>(rawJson, JsonSettings);

                if (fetchDto != null && fetchDto.Results != null && fetchDto.Results.Count > 0)
                {
                    var resultDto = new TestInsertDto
                    {
                        TestMaster = fetchDto.TestMaster ?? new test_master { tcode = fetchDto.TCode },
                        ResultRows = fetchDto.Results
                    };

                    // Ensure tcode is set on every resultMaster
                    NormaliseResultRows(resultDto);
                    return resultDto;
                }

                // ── Fallback: parse raw JObject ─────────────────────────────────
                var outer = JsonConvert.DeserializeObject<JObject>(rawJson, JsonSettings);
                if (outer == null) return null;

                // Find the results array under any of these key names
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
            catch (Exception ex)
            {
                Console.WriteLine($"[TestService] GetTestResult error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// After deserialising a fetch response, ensure every row has correct
        /// tcode, non-null collections, and consistent resultvaluetype strings.
        /// This is also where we preserve the real trcode from the DB.
        /// </summary>
        private void NormaliseResultRows(TestInsertDto dto)
        {
            if (dto.ResultRows == null) return;
            foreach (var row in dto.ResultRows)
            {
                // Preserve trcode exactly as returned by the API — do NOT zero it out
                row.ResultMaster.tcode = dto.TestMaster.tcode;

                // Prefer col2 as display name if cellcontent is empty
                if (string.IsNullOrWhiteSpace(row.ResultMaster.cellcontent))
                    row.ResultMaster.cellcontent = row.ResultMaster.col2;

                // Ensure ResultProperties exists
                row.ResultProperties ??= new test_result_properties
                {
                    trpid = Guid.NewGuid(),
                    testresultid = row.ResultMaster.testresultid,
                    resultvaluetype = "Number"
                };

                // Map shorthand resultvaluetype codes the backend may return
                row.ResultProperties.resultvaluetype = row.ResultProperties.resultvaluetype switch
                {
                    // API shorthand codes → UI display names
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
                    // API full-name aliases that differ from UI names
                    "FixedText" => "Fixed Type",
                    "Numeric"   => "Number",
                    _           => row.ResultProperties.resultvaluetype ?? "Number"
                };

                row.CalculatedFormulas ??= new();
                row.DetailedNormalValues ??= new();
                row.TextNormalValues ??= new();
            }
        }

        // ─── Test Type Master ───────────────────────────────────────────────────

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

        // ─── Masters ───────────────────────────────────────────────────────────

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
        public async Task<List<MachineMaster>> GetMachinesAsync()
        {
            ConfigureHeaders();
            try
            {
                var response = await _http.GetAsync("MachineMaster/get");
                if (!response.IsSuccessStatusCode) return new();
                var rawJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<MachineMaster>>(rawJson, JsonSettings) ?? new();
            }
            catch { return new(); }
        }

        public async Task<bool> InsertMachineAsync(MachineMaster machine)
        {
            ConfigureHeaders();
            var response = await _http.PostAsJsonAsync("MachineMaster/insert", machine);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateMachineAsync(MachineMaster machine)
        {
            ConfigureHeaders();
            var response = await _http.PostAsJsonAsync("MachineMaster/update", machine);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> SoftDeleteMachineAsync(int mccode)
        {
            ConfigureHeaders();
            var response = await _http.PostAsJsonAsync($"MachineMaster/softdelete?mccode={mccode}", new { });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteMachineAsync(int mccode)
        {
            ConfigureHeaders();
            var response = await _http.DeleteAsync($"MachineMaster/delete?mccode={mccode}");
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

        // Report Methods CRUD
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

        // Units CRUD
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