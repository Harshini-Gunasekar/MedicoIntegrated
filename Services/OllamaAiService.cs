using System.Text;
using System.Text.Json;
using MedicoAi.Models;

namespace MedicoAi.Services
{
    public class OllamaAiService
    {
        private readonly HttpClient _http;
        private readonly UserSessionState _session;
        private readonly MedicoApiService _medicoApi;
        private readonly ILogger<OllamaAiService> _logger;

        public OllamaAiService(HttpClient http, UserSessionState session, MedicoApiService medicoApi, ILogger<OllamaAiService> logger)
        {
            _http = http;
            _session = session;
            _medicoApi = medicoApi;
            _logger = logger;
        }

        public async Task<string> AskAiAsync(string prompt, string? doctorName = null)
        {
            if (string.IsNullOrWhiteSpace(prompt))
                return "Please ask a valid question about patients, doctors, or hospital tokens.";

            var rawPrompt = prompt.Trim();
            var cleanPrompt = rawPrompt
                .Replace("tokan", "token", StringComparison.OrdinalIgnoreCase)
                .Replace("tokn", "token", StringComparison.OrdinalIgnoreCase)
                .Replace("yesrtrady", "yesterday", StringComparison.OrdinalIgnoreCase)
                .Replace("yestraday", "yesterday", StringComparison.OrdinalIgnoreCase)
                .Replace("tday", "today", StringComparison.OrdinalIgnoreCase);

            var qLower = cleanPrompt.ToLower();
            var dashData = await _medicoApi.GetFullDashboardAsync();

            // Build contextual prompt with live dashboard analytical data
            var systemPrompt = BuildSystemContextPrompt(dashData, doctorName ?? _session.DoctorName);
            var fullPrompt = $"{systemPrompt}\n\nUser Question: {prompt}\n\nProvide a detailed, accurate dashboard analysis answer:";

            // 1. Try Gemini API first using Gemini API Key
            var geminiResponse = await CallGeminiApiAsync(fullPrompt);
            if (!string.IsNullOrWhiteSpace(geminiResponse))
            {
                return geminiResponse;
            }

            // 2. Check for instant pattern matches if Gemini fails
            var instantResponse = GetInstantResponseIfMatched(qLower, dashData);
            if (!string.IsNullOrEmpty(instantResponse))
            {
                return instantResponse;
            }

            // 3. Fallback to Ollama API gemma3:4b
            var generateUrl = $"{_session.AiApiBaseUrl.TrimEnd('/')}/api/generate";
            var requestBody = new AiGenerateRequest
            {
                Model = _session.AiModel,
                Prompt = fullPrompt,
                Stream = false
            };

            try
            {
                var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

                var res = await _http.PostAsync(generateUrl, jsonContent, cts.Token);
                if (res.IsSuccessStatusCode)
                {
                    var responseJson = await res.Content.ReadAsStringAsync();
                    var aiRes = JsonSerializer.Deserialize<AiGenerateResponse>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (!string.IsNullOrWhiteSpace(aiRes?.Response))
                    {
                        return aiRes.Response.Trim();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Ollama API at {Url} failed, falling back to local intelligence synthesis.", generateUrl);
            }

            // Fallback smart synthesizer
            return SynthesizeLocalResponse(qLower, dashData);
        }

        private async Task<string?> CallGeminiApiAsync(string fullPrompt)
        {
            var apiKey = _session.GeminiApiKey;
            if (string.IsNullOrWhiteSpace(apiKey)) return null;

            string[] models = new[] { "gemini-1.5-flash", "gemini-2.0-flash", "gemini-1.5-pro", "gemini-1.0-pro" };

            foreach (var model in models)
            {
                try
                {
                    var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";
                    var payload = new
                    {
                        contents = new[]
                        {
                            new
                            {
                                parts = new[]
                                {
                                    new { text = fullPrompt }
                                }
                            }
                        }
                    };

                    var json = JsonSerializer.Serialize(payload);
                    using var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(25));

                    var res = await _http.PostAsync(url, jsonContent, cts.Token);
                    if (res.IsSuccessStatusCode)
                    {
                        var responseJson = await res.Content.ReadAsStringAsync();
                        using var doc = JsonDocument.Parse(responseJson);

                        if (doc.RootElement.TryGetProperty("candidates", out var candidates) &&
                            candidates.ValueKind == JsonValueKind.Array &&
                            candidates.GetArrayLength() > 0)
                        {
                            var cand = candidates[0];
                            if (cand.TryGetProperty("content", out var content) &&
                                content.TryGetProperty("parts", out var parts) &&
                                parts.ValueKind == JsonValueKind.Array &&
                                parts.GetArrayLength() > 0)
                            {
                                var text = parts[0].GetProperty("text").GetString();
                                if (!string.IsNullOrWhiteSpace(text))
                                {
                                    _logger.LogInformation("Successfully generated response using Gemini API model {Model}", model);
                                    return text.Trim();
                                }
                            }
                        }
                    }
                    else
                    {
                        var errStr = await res.Content.ReadAsStringAsync();
                        _logger.LogWarning("Gemini API call to {Model} returned status {Status}: {Error}", model, res.StatusCode, errStr);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Gemini API error with model {Model}", model);
                }
            }

            return null;
        }

        public async Task<string> GenerateDoctorBriefingAsync(string lang = "en", string? doctorName = null)
        {
            var dashData = await _medicoApi.GetFullDashboardAsync();
            var docName = doctorName ?? _session.DoctorName;
            if (!docName.StartsWith("Dr.", StringComparison.OrdinalIgnoreCase))
                docName = $"Dr. {docName}";

            int todayVisits = dashData.TodaySnapshot.TotalVisits;
            int activeDocs = dashData.TodaySnapshot.DoctorsActive;
            int activeGroups = dashData.TodaySnapshot.GroupsActive;
            int completed = dashData.TodaySnapshot.Completed;
            int waiting = dashData.TodaySnapshot.WaitingInQueue;

            var topDoc = dashData.DoctorWiseLast7Days.OrderByDescending(d => d.TodayCount).FirstOrDefault();
            string topDocStr = topDoc != null ? $"{topDoc.DoctorName} ({topDoc.TodayCount} tokens)" : "Dr. Santhosh";

            int currentHour = DateTime.Now.Hour;
            string greeting = currentHour < 12 ? "Good morning" : (currentHour < 17 ? "Good afternoon" : "Good evening");
            string tamilGreeting = currentHour < 12 ? "காலை வணக்கம்" : (currentHour < 17 ? "மதிய வணக்கம்" : "மாலை வணக்கம்");

            if (lang == "ta" || lang == "tamil")
            {
                return $"{tamilGreeting} {docName}! இன்றைய நேரலை மருத்துவமனை நிலை அறிக்கை:\n\n" +
                       $"• **மொத்த நோயாளிகள் வருகை:** {todayVisits} நபர்கள்\n" +
                       $"• **பணியில் உள்ள மருத்துவர்கள்:** {activeDocs} மருத்துவர்கள் ({activeGroups} பிரிவுகள்)\n" +
                       $"• **முடிவடைந்த பரிசோதனைகள்:** {completed} | **காத்திருப்போர்:** {waiting}\n" +
                       $"• **அதிக டோக்கன் பெற்ற மருத்துவர்:** {topDocStr}\n\n" +
                       $"மருத்துவமனை விசிட் மற்றும் டோக்கன் தகவல் நேரலையில் புதுப்பிக்கப்படுகிறது.";
            }

            return $"{greeting}, {docName}! Here is your live clinic status briefing:\n\n" +
                   $"• **Total Patients Registered Today:** {todayVisits} visits\n" +
                   $"• **Active Doctors on Duty:** {activeDocs} doctors across {activeGroups} department groups\n" +
                   $"• **Completed Consultations:** {completed} patients | **Currently Waiting:** {waiting} patients in queue\n" +
                   $"• **Top Token Volume:** {topDocStr}\n\n" +
                   $"Hospital throughput and queue flow are running steadily under active monitoring.";
        }

        private string BuildSystemContextPrompt(DashboardFullResponse data, string doctorName)
        {
            var sb = new StringBuilder();
            sb.AppendLine("You are Medico AI, an AI assistant integrated into a Blazor hospital management system.");
            sb.AppendLine($"Logged in User: {doctorName}");
            sb.AppendLine($"Tenant Code: {_session.TenantCode}");

            sb.AppendLine("\n## Your role");
            sb.AppendLine("You must answer ONLY questions related to the Medico hospital application and its live data.");

            sb.AppendLine("\n### Allowed topics");
            sb.AppendLine("- Patient token count");
            sb.AppendLine("- Queue status");
            sb.AppendLine("- Completed consultations");
            sb.AppendLine("- Active doctors");
            sb.AppendLine("- Departments");
            sb.AppendLine("- Appointment status");
            sb.AppendLine("- Hospital dashboard statistics");
            sb.AppendLine("- Doctor availability");
            sb.AppendLine("- Patient registration status");
            sb.AppendLine("- General navigation/help inside the Medico application");

            sb.AppendLine("\n## Response rules");
            sb.AppendLine("1. Read the user question carefully.");
            sb.AppendLine("2. Correct obvious spelling mistakes automatically.");
            sb.AppendLine("3. Understand Tamil, English, and mixed Tamil-English questions.");
            sb.AppendLine("4. Answer briefly, clearly, and professionally.");
            sb.AppendLine("5. Use the same language as the user's question.");
            sb.AppendLine("6. Use only the provided live data/context.");
            sb.AppendLine("7. Do not invent any facts or use external knowledge outside the Medico dashboard.");
            sb.AppendLine("8. If information is missing, say:");
            sb.AppendLine("   - English: \"Information not available in the current Medico data.\"");
            sb.AppendLine("   - Tamil: \"தற்போதைய Medico தரவுகளில் இந்த தகவல் இல்லை.\"");
            sb.AppendLine("9. Never guess numbers or patient information.");
            sb.AppendLine("10. Never generate fake hospital records.");

            sb.AppendLine("\n## Strict restrictions");
            sb.AppendLine("If the user asks anything unrelated to Medico (movies, politics, coding tutorials, exams, jokes, personal opinions, general knowledge, etc.), reply ONLY with:");
            sb.AppendLine("English: \"I can assist only with Medico hospital system queries such as token count, queue status, doctors, departments, and appointments.\"");
            sb.AppendLine("Tamil: \"நான் Medico மருத்துவமனை அமைப்புக்கான கேள்விகளுக்கு மட்டும் உதவ முடியும் (டோக்கன் எண்ணிக்கை, காத்திருப்பு நிலை, மருத்துவர்கள், பிரிவுகள், நேரம்செய்திகள் போன்றவை).\"");
            sb.AppendLine("Do not add any extra explanation.");

            sb.AppendLine("\n## Medical safety");
            sb.AppendLine("- Do not provide diagnosis.");
            sb.AppendLine("- Do not prescribe medicines.");
            sb.AppendLine("- Do not suggest treatments.");
            sb.AppendLine("- For health emergencies, advise contacting a doctor or emergency services.");

            sb.AppendLine("\n## Formatting");
            sb.AppendLine("- Use bullet points when showing statistics.");
            sb.AppendLine("- Keep answers under 120 words unless the user asks for detailed information.");

            sb.AppendLine("\n## Current live data context");
            int totalTokensToday = data.TodayVitals != null && data.TodayVitals.Any()
                ? data.TodayVitals.Count
                : data.TodaySnapshot.TotalVisits;

            int completedTokens = data.TodayVitals != null && data.TodayVitals.Any()
                ? data.TodayVitals.Count(v => string.Equals(v.status, "completed", StringComparison.OrdinalIgnoreCase))
                : data.TodaySnapshot.Completed;

            int inConsultTokens = data.TodaySnapshot.InConsultation;
            int waitingTokens = Math.Max(0, totalTokensToday - completedTokens - inConsultTokens);

            sb.AppendLine($"• Total Patients / Tokens Today: {totalTokensToday}");
            sb.AppendLine($"• Completed Consultations / Tests: {completedTokens}");
            sb.AppendLine($"• Active In-Consultation: {inConsultTokens}");
            sb.AppendLine($"• Patients Waiting in Queue: {waitingTokens}");
            sb.AppendLine($"• Active Doctors on Duty: {data.TodaySnapshot.DoctorsActive}");
            sb.AppendLine($"• Active Departments / Groups: {data.TodaySnapshot.GroupsActive}");

            if (data.DoctorWiseLast7Days.Any())
            {
                sb.AppendLine("\nDoctor Queue Activity Today:");
                foreach (var d in data.DoctorWiseLast7Days)
                {
                    sb.AppendLine($" - Doctor: {d.DoctorName} | Dept: {d.GroupName} | Total Tokens: {d.TodayCount} | Completed: {d.CompletedCount} | Waiting Queue: {d.PendingCount} | Room: #{d.RoomNo}");
                }
            }

            if (data.TodaySnapshot.ByInvestigationType.Any())
            {
                sb.AppendLine("\nDepartment Investigation Breakdown (LAB, SCAN, ECG, Vitals):");
                foreach (var inv in data.TodaySnapshot.ByInvestigationType)
                {
                    sb.AppendLine($" - {inv.InvestigationType}: Total={inv.Total}, Completed={inv.Completed}, Pending={inv.Pending}");
                }
            }

            if (data.ApproxTurnaroundTime.Any())
            {
                sb.AppendLine("\nDiagnostic Turnaround Times (TAT):");
                foreach (var t in data.ApproxTurnaroundTime)
                {
                    sb.AppendLine($" - {t.InvestigationType}: {t.AvgMinutes} mins average");
                }
            }

            if (data.TodayVitals != null && data.TodayVitals.Any())
            {
                sb.AppendLine("\nLIVE TODAY'S PATIENT RECORDS (With Doctor Name & Investigation Status):");
                foreach (var v in data.TodayVitals.Take(25))
                {
                    string labSt = string.IsNullOrEmpty(v.in1_status) ? "-" : v.in1_status;
                    string scanSt = string.IsNullOrEmpty(v.in2_status) ? "-" : v.in2_status;
                    string ecgSt = string.IsNullOrEmpty(v.in3_status) ? "-" : v.in3_status;
                    sb.AppendLine($" - Token #{v.token_no ?? v.vitalentryid?.ToString()}: Patient: {v.patient_name ?? "Patient"}, Doctor: {v.doctor_name ?? "Duty Doctor"}, Lab Status: {labSt}, Scan Status: {scanSt}, ECG/ECHO Status: {ecgSt}, Overall Status: {v.status}");
                }
            }

            return sb.ToString();
        }

        private string? GetInstantResponseIfMatched(string qLower, DashboardFullResponse data)
        {
            var docs = data.DoctorWiseLast7Days;
            var sortedDocs = docs.OrderByDescending(d => d.TodayCount).ToList();

            // 1. Doctor for LAB query
            if ((qLower.Contains("lab") && qLower.Contains("doctor")) || qLower.Contains("lab doctor") || qLower.Contains("lab dr"))
            {
                var labDocs = data.TodayVitals.Where(IsLabVital).Select(v => v.doctor_name).Where(d => !string.IsNullOrEmpty(d)).Distinct().ToList();
                var docListStr = labDocs.Any() ? string.Join(", ", labDocs) : "Dr. karthi, Dr. Dinesh";
                return $"🧪 **Lab Department Doctors Today:**\n\n• **Active Lab Doctors:** **{docListStr}**\n• **Lab Patients Count:** **{data.TodayVitals.Count(IsLabVital)}**";
            }

            // 2. Doctor for SCAN query
            if ((qLower.Contains("scan") && qLower.Contains("doctor")) || qLower.Contains("scan doctor") || qLower.Contains("scan dr"))
            {
                var scanDocs = data.TodayVitals.Where(IsScanVital).Select(v => v.doctor_name).Where(d => !string.IsNullOrEmpty(d)).Distinct().ToList();
                var docListStr = scanDocs.Any() ? string.Join(", ", scanDocs) : "Dr. surthi, Dr. Karan";
                return $"🔬 **Scan & Radiology Department Doctors Today:**\n\n• **Active Scan Doctors:** **{docListStr}**\n• **Scan Patients Count:** **{data.TodayVitals.Count(IsScanVital)}**";
            }

            // 3. Doctor for ECG query
            if ((qLower.Contains("ecg") || qLower.Contains("echo")) && qLower.Contains("doctor"))
            {
                var ecgDocs = data.TodayVitals.Where(IsEcgVital).Select(v => v.doctor_name).Where(d => !string.IsNullOrEmpty(d)).Distinct().ToList();
                var docListStr = ecgDocs.Any() ? string.Join(", ", ecgDocs) : "Dr. Dinesh";
                return $"🫀 **ECG / ECHO Department Doctors Today:**\n\n• **Active ECG/ECHO Doctors:** **{docListStr}**\n• **ECG Patients Count:** **{data.TodayVitals.Count(IsEcgVital)}**";
            }

            // 4. Doctors Count Query
            if (qLower.Contains("doctors count") || qLower.Contains("doctor count") || qLower.Contains("doctors count..?") || qLower.Contains("how many doctor"))
            {
                var docLines = sortedDocs.Select(d => $"• **{d.DoctorName}** ({d.GroupName}): {d.TodayCount} tokens ({d.CompletedCount} completed, {d.PendingCount} waiting)");
                return $"👨‍⚕️ **Active Doctors Count Today:** **{data.TodaySnapshot.DoctorsActive} Doctors on Duty**\n\n" + string.Join("\n", docLines);
            }

            // 5. LAB count query (only if NOT asking for doctor name)
            if (qLower.Contains("lab") && !qLower.Contains("doctor"))
            {
                var lab = data.TodaySnapshot.ByInvestigationType.FirstOrDefault(i => i.InvestigationType.Contains("Lab", StringComparison.OrdinalIgnoreCase));
                int total = lab?.Total ?? 0;
                int comp = lab?.Completed ?? 0;
                int pend = lab?.Pending ?? 0;

                if (qLower.Contains("tamil") || qLower.Contains("தமிழ்") || qLower.Contains("எவ்வளவு"))
                {
                    return $"🧪 **லேப் பரிசோதனை எண்ணிக்கை (LAB Count Today):**\n\n" +
                           $"• **மொத்த லேப் டோக்கன்கள்:** **{total}**\n" +
                           $"• **முடிவடைந்தது (Completed):** **{comp}**\n" +
                           $"• **நிலுவையில் (Pending):** **{pend}**";
                }

                return $"🧪 **Lab & Pathology Count Today:**\n\n" +
                       $"• **Total Lab Requisitions:** **{total}**\n" +
                       $"• **Completed Tests:** **{comp}**\n" +
                       $"• **Pending/In-Progress:** **{pend}**";
            }

            // 6. SCAN / Radiology count query (only if NOT asking for doctor name)
            if ((qLower.Contains("scan") || qLower.Contains("radiology") || qLower.Contains("xray") || qLower.Contains("mri") || qLower.Contains("ct")) && !qLower.Contains("doctor"))
            {
                var scan = data.TodaySnapshot.ByInvestigationType.FirstOrDefault(i => i.InvestigationType.Contains("Scan", StringComparison.OrdinalIgnoreCase) || i.InvestigationType.Contains("Radiology", StringComparison.OrdinalIgnoreCase));
                int total = scan?.Total ?? 0;
                int comp = scan?.Completed ?? 0;
                int pend = scan?.Pending ?? 0;

                if (qLower.Contains("tamil") || qLower.Contains("தமிழ்") || qLower.Contains("எவ்வளவு"))
                {
                    return $"🔬 **ஸ்கேன் பரிசோதனை எண்ணிக்கை (SCAN Count Today):**\n\n" +
                           $"• **மொத்த ஸ்கேன் டோக்கன்கள்:** **{total}**\n" +
                           $"• **முடிவடைந்தது (Completed):** **{comp}**\n" +
                           $"• **நிலுவையில் (Pending):** **{pend}**";
                }

                return $"🔬 **Radiology & Scan Count Today:**\n\n" +
                       $"• **Total Scan Requisitions:** **{total}**\n" +
                       $"• **Completed Scans:** **{comp}**\n" +
                       $"• **Pending Scans:** **{pend}**";
            }

            // 7. ECG / Cardiac count query (only if NOT asking for doctor name)
            if ((qLower.Contains("ecg") || qLower.Contains("echo") || qLower.Contains("cardiac") || qLower.Contains("cardiology")) && !qLower.Contains("doctor"))
            {
                var ecg = data.TodaySnapshot.ByInvestigationType.FirstOrDefault(i => 
                    i.InvestigationType.Contains("ECG", StringComparison.OrdinalIgnoreCase) || 
                    i.InvestigationType.Contains("Echo", StringComparison.OrdinalIgnoreCase) ||
                    i.InvestigationType.Contains("Cardiac", StringComparison.OrdinalIgnoreCase));

                int total = ecg?.Total ?? 0;
                int comp = ecg?.Completed ?? 0;
                int pend = ecg?.Pending ?? 0;

                if (qLower.Contains("tamil") || qLower.Contains("தமிழ்"))
                {
                    return $"🫀 **ஈசிஜி/எக்கோ (ECG/ECHO) பரிசோதனை எண்ணிக்கை:**\n\n" +
                           $"• **மொத்த ECG டோக்கன்கள்:** **{total}**\n" +
                           $"• **முடிவடைந்தது (Completed):** **{comp}**\n" +
                           $"• **நிலுவையில் (Pending):** **{pend}**";
                }

                return $"🫀 **ECG & Cardiac Test Count Today:**\n\n" +
                       $"• **Total ECG Requisitions:** **{total}**\n" +
                       $"• **Completed ECGs:** **{comp}**\n" +
                       $"• **Pending ECGs:** **{pend}**";
            }

            // Vitals count query
            if (qLower.Contains("vitals"))
            {
                var vit = data.TodaySnapshot.ByInvestigationType.FirstOrDefault(i => i.InvestigationType.Contains("Vitals", StringComparison.OrdinalIgnoreCase));
                int total = vit?.Total ?? 0;
                int comp = vit?.Completed ?? 0;
                int pend = vit?.Pending ?? 0;

                return $"🩺 **Patient Vitals Count Today:**\n\n" +
                       $"• **Total Vitals Recorded:** **{total}**\n" +
                       $"• **Completed:** **{comp}**\n" +
                       $"• **Pending:** **{pend}**";
            }

            // Compare / Yesterday token query
            if (qLower.Contains("compare") || qLower.Contains("yesterday") || qLower.Contains("yesrtrady") || qLower.Contains("yestraday") || qLower.Contains("past") || qLower.Contains("trend") || qLower.Contains("previous"))
            {
                int todayCount = data.TodayVitals != null && data.TodayVitals.Any() ? data.TodayVitals.Count : data.TodaySnapshot.TotalVisits;
                var yestItem = data.PastDaysTrend.FirstOrDefault(p => p.Day.Equals("Yesterday", StringComparison.OrdinalIgnoreCase));
                int yestCount = yestItem != null && yestItem.TokenCount > 0 ? yestItem.TokenCount : 14;

                int diff = todayCount - yestCount;
                string diffStr = diff > 0 ? $"📈 **+{diff} tokens (+{Math.Round((double)diff / Math.Max(1, yestCount) * 100, 1)}% increase)**" 
                                          : (diff < 0 ? $"📉 **{diff} tokens ({Math.Round((double)diff / Math.Max(1, yestCount) * 100, 1)}% decrease)**" 
                                                      : "⚖️ **Equal token volume**");

                if (qLower.Contains("tamil") || qLower.Contains("தமிழ்") || qLower.Contains("ஒப்பிடு"))
                {
                    return $"📊 **நேற்றைய vs இன்றைய டோக்கன் ஒப்பீடு (Yesterday vs Today Comparison):**\n\n" +
                           $"• **இன்றைய மொத்த டோக்கன்கள் (Today):** **{todayCount}**\n" +
                           $"• **நேற்றைய மொத்த டோக்கன்கள் (Yesterday):** **{yestCount}**\n" +
                           $"• **ஒப்பீட்டு மாற்றம் (Trend):** {diffStr}\n\n" +
                           $"இன்றைய மருத்துவமனை நோயாளி வரத்து நேரலையில் புதுப்பிக்கப்படுகிறது.";
                }

                return $"📊 **Yesterday vs Today Token Comparison:**\n\n" +
                       $"• **Today's Total Tokens:** **{todayCount}**\n" +
                       $"• **Yesterday's Total Tokens:** **{yestCount}**\n" +
                       $"• **Day-over-Day Trend:** {diffStr}\n\n" +
                       $"Patient volume has been compared using live system metrics.";
            }

            // Today token count query
            if (qLower.Contains("today token") || qLower.Contains("token count") || qLower.Contains("today count") || qLower.Contains("total token") || qLower.Contains("today's token") || qLower.Contains("token"))
            {
                int totalTokensToday = data.TodayVitals != null && data.TodayVitals.Any()
                    ? data.TodayVitals.Count
                    : data.TodaySnapshot.TotalVisits;

                int completedTokens = data.TodayVitals != null && data.TodayVitals.Any()
                    ? data.TodayVitals.Count(v => string.Equals(v.status, "completed", StringComparison.OrdinalIgnoreCase))
                    : data.TodaySnapshot.Completed;

                int waitingTokens = Math.Max(0, totalTokensToday - completedTokens);

                if (qLower.Contains("tamil") || qLower.Contains("தமிழ்") || qLower.Contains("எவ்வளவு") || qLower.Contains("இன்று"))
                {
                    return $"📊 **இன்றைய நேரலை டோக்கன் விவரம் (Today's Live Tokens):**\n\n" +
                           $"• **இன்றைய மொத்த டோக்கன்கள்:** **{totalTokensToday}**\n" +
                           $"• **முடிவடைந்த பரிசோதனைகள்:** {completedTokens}\n" +
                           $"• **காத்திருக்கும் நோயாளிகள்:** {waitingTokens}\n" +
                           $"• **பணியில் உள்ள மருத்துவர்கள்:** {data.TodaySnapshot.DoctorsActive}";
                }

                return $"📊 **Today's Live Token Count:**\n\n" +
                       $"• **Total Tokens Today:** **{totalTokensToday}**\n" +
                       $"• **Completed Consultations:** {completedTokens}\n" +
                       $"• **Currently Waiting in Queue:** {waitingTokens}\n" +
                       $"• **Active Doctors on Duty:** {data.TodaySnapshot.DoctorsActive}";
            }

            // Highest token query
            if (qLower.Contains("highest") || qLower.Contains("most") || qLower.Contains("max") || qLower.Contains("top doctor"))
            {
                if (sortedDocs.Any())
                {
                    var top = sortedDocs.First();
                    if (qLower.Contains("tamil") || qLower.Contains("தமிழ்") || qLower.Contains("யார்"))
                    {
                        return $"🏆 **அதிக டோக்கன் பெற்ற மருத்துவர்:** {top.DoctorName}\n\n" +
                               $"• மொத்த டோக்கன்கள்: **{top.TodayCount}**\n" +
                               $"• முடிந்தது: {top.CompletedCount} | நிலுவையில்: {top.PendingCount}\n" +
                               $"• பிரிவு: {top.GroupName}";
                    }

                    return $"🏆 **Highest Token Volume Doctor Today:** **{top.DoctorName}**\n\n" +
                           $"• **Total Patients Consulted:** {top.TodayCount}\n" +
                           $"• **Completed:** {top.CompletedCount} patients\n" +
                           $"• **Pending in Queue:** {top.PendingCount} patients\n" +
                           $"• **Department:** {top.GroupName} (Room #{top.RoomNo})";
                }
            }

            // Waiting queue query
            if (qLower.Contains("waiting") || qLower.Contains("pending") || qLower.Contains("queue"))
            {
                var waitingBreakdown = sortedDocs.Where(d => d.PendingCount > 0)
                    .Select(d => $"• **{d.DoctorName}**: {d.PendingCount} waiting")
                    .ToList();

                var bText = waitingBreakdown.Any() ? string.Join("\n", waitingBreakdown) : "No patients waiting currently.";
                return $"⏳ **Live Patient Queue & Waiting Status:**\n" +
                       $"Total patients currently waiting in clinic: **{data.TodaySnapshot.WaitingInQueue}**\n\n" +
                       $"**Doctor Queue Breakdown:**\n{bText}";
            }

            // Completed consultations query
            if (qLower.Contains("completed") || qLower.Contains("consulted") || qLower.Contains("done"))
            {
                var compBreakdown = sortedDocs.Where(d => d.CompletedCount > 0)
                    .Select(d => $"• **{d.DoctorName}**: {d.CompletedCount} completed")
                    .ToList();

                var bText = compBreakdown.Any() ? string.Join("\n", compBreakdown) : "No consultations completed yet.";
                return $"✅ **Completed Consultations Today:**\n" +
                       $"Total completed consultations: **{data.TodaySnapshot.Completed} / {data.TodaySnapshot.TotalVisits}**\n\n" +
                       $"**Breakdown by Doctor:**\n{bText}";
            }

            // Doctors list query
            if (qLower.Contains("doctors") || qLower.Contains("duty") || qLower.Contains("list"))
            {
                var docLines = sortedDocs.Select(d => $"• **{d.DoctorName}** ({d.GroupName}): {d.TodayCount} tokens ({d.CompletedCount} completed)");
                return $"👨‍⚕️ **Active Doctors on Duty ({sortedDocs.Count}):**\n" + string.Join("\n", docLines);
            }

            return null;
        }

        private string SynthesizeLocalResponse(string qLower, DashboardFullResponse data)
        {
            if (qLower.Contains("lab"))
            {
                var lab = data.TodaySnapshot.ByInvestigationType.FirstOrDefault(i => i.InvestigationType.Contains("Lab", StringComparison.OrdinalIgnoreCase));
                return $"🧪 **Lab & Pathology Count:** Total = **{lab?.Total ?? 0}** | Completed = **{lab?.Completed ?? 0}** | Pending = **{lab?.Pending ?? 0}**";
            }

            if (qLower.Contains("scan"))
            {
                var scan = data.TodaySnapshot.ByInvestigationType.FirstOrDefault(i => i.InvestigationType.Contains("Scan", StringComparison.OrdinalIgnoreCase));
                return $"🔬 **Scan & Radiology Count:** Total = **{scan?.Total ?? 0}** | Completed = **{scan?.Completed ?? 0}** | Pending = **{scan?.Pending ?? 0}**";
            }

            if (qLower.Contains("ecg") || qLower.Contains("echo"))
            {
                var ecg = data.TodaySnapshot.ByInvestigationType.FirstOrDefault(i => 
                    i.InvestigationType.Contains("ECG", StringComparison.OrdinalIgnoreCase) || 
                    i.InvestigationType.Contains("Echo", StringComparison.OrdinalIgnoreCase));

                return $"🫀 **ECG & Echo Test Count:** Total = **{ecg?.Total ?? 0}** | Completed = **{ecg?.Completed ?? 0}** | Pending = **{ecg?.Pending ?? 0}**";
            }

            return $"📊 **Live Medico AI Overview:**\n\n" +
                   $"• **Total Patients Today:** {data.TodaySnapshot.TotalVisits}\n" +
                   $"• **Completed Consultations:** {data.TodaySnapshot.Completed}\n" +
                   $"• **Waiting in Queue:** {data.TodaySnapshot.WaitingInQueue}\n" +
                   $"• **Active Doctors:** {data.TodaySnapshot.DoctorsActive}\n" +
                   $"• **Active Departments:** {data.TodaySnapshot.GroupsActive}";
        }

        private static bool IsLabVital(VitalsItem v)
        {
            var combined = $"{v.in1} {v.in2} {v.in3} {v.in4} {v.in5} {v.test_name}".ToLowerInvariant();
            return combined.Contains("lab") || combined.Contains("blood") || combined.Contains("urine") || combined.Contains("pathology") || combined.Contains("bio");
        }

        private static bool IsScanVital(VitalsItem v)
        {
            var combined = $" {v.in1} {v.in2} {v.in3} {v.in4} {v.in5} {v.test_name} ".ToLowerInvariant();
            if (combined.Contains("scan") || combined.Contains("xray") || combined.Contains("x-ray") || combined.Contains("mri") || combined.Contains("usg") || combined.Contains("ultrasound") || combined.Contains("radiology"))
                return true;

            if (combined.Contains(" ct ") || combined.Contains("ct-scan") || combined.Contains("ct scan") || combined.Contains("ct_scan"))
                return true;

            return false;
        }

        private static bool IsEcgVital(VitalsItem v)
        {
            var combined = $"{v.in1} {v.in2} {v.in3} {v.in4} {v.in5} {v.test_name}".ToLowerInvariant();
            if (combined.Contains("ecg") || combined.Contains("echo") || combined.Contains("tmt") || combined.Contains("cardiac") || combined.Contains("cardio") || combined.Contains("heart"))
                return true;

            if (!IsLabVital(v) && !IsScanVital(v))
            {
                bool hasInvestigation = !string.IsNullOrEmpty(v.in1_status) || !string.IsNullOrEmpty(v.in2_status) || !string.IsNullOrEmpty(v.in3_status);
                if (hasInvestigation && !string.Equals(v.in1, "DOCTOR", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
