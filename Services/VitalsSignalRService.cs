using System.Net.Http.Headers;
using System.Text.Json;
using MedicoAi.Models;

namespace MedicoAi.Services
{
    public class VitalsSignalRService : IDisposable
    {
        private readonly HttpClient _http;
        private readonly UserSessionState _session;
        private readonly ILogger<VitalsSignalRService> _logger;
        private CancellationTokenSource? _cts;
        private PeriodicTimer? _timer;
        private bool _isStreaming = false;

        public event Action<List<VitalsItem>>? OnVitalsUpdated;
        public event Action<string>? OnStreamStatusChanged;

        public bool IsStreaming => _isStreaming;
        public DateTime? LastHitTime { get; private set; }
        public int TotalHits { get; private set; } = 0;
        public List<VitalsItem> LatestVitals { get; private set; } = new();
        public string ConnectionState { get; private set; } = "Disconnected";

        public VitalsSignalRService(HttpClient http, UserSessionState session, ILogger<VitalsSignalRService> logger)
        {
            _http = http;
            _session = session;
            _logger = logger;
        }

        public void StartLiveStream(int pollIntervalSeconds = 3)
        {
            if (_isStreaming) return;

            _isStreaming = true;
            ConnectionState = "SignalR Streaming Active";
            OnStreamStatusChanged?.Invoke(ConnectionState);

            _cts = new CancellationTokenSource();
            _timer = new PeriodicTimer(TimeSpan.FromSeconds(pollIntervalSeconds));

            _ = Task.Run(async () =>
            {
                // Immediate initial fetch
                await FetchAndNotifyVitalsAsync();

                while (_timer != null && await _timer.WaitForNextTickAsync(_cts.Token))
                {
                    if (_cts.IsCancellationRequested) break;
                    await FetchAndNotifyVitalsAsync();
                }
            }, _cts.Token);
        }

        public async Task<List<VitalsItem>> FetchAndNotifyVitalsAsync()
        {
            var code = _session.TenantCode;
            var requestUrl = $"{_session.ApiBaseUrl.TrimEnd('/')}/api/Vitals/get?tenant_code={code}";

            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                req.Headers.Add("tenant_code", code);
                req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var res = await _http.SendAsync(req);
                LastHitTime = DateTime.Now;
                TotalHits++;

                if (res.IsSuccessStatusCode)
                {
                    var json = await res.Content.ReadAsStringAsync();
                    var items = JsonSerializer.Deserialize<List<VitalsItem>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    
                    if (items != null)
                    {
                        LatestVitals = items;
                        _logger.LogInformation("[SignalR Live Stream] Hit #{Count} to {Url} -> Received {CountItems} items at {Time}", TotalHits, requestUrl, items.Count, LastHitTime.Value.ToString("HH:mm:ss"));
                        OnVitalsUpdated?.Invoke(LatestVitals);
                        return LatestVitals;
                    }
                }
                else
                {
                    _logger.LogWarning("[SignalR Live Stream] Hit #{Count} to {Url} returned status code {Status}", TotalHits, requestUrl, res.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SignalR Live Stream] Error during live stream hit to {Url}", requestUrl);
            }

            return LatestVitals;
        }

        public void StopLiveStream()
        {
            _isStreaming = false;
            ConnectionState = "Disconnected";
            _cts?.Cancel();
            _timer?.Dispose();
            _timer = null;
            OnStreamStatusChanged?.Invoke(ConnectionState);
        }

        public void Dispose()
        {
            StopLiveStream();
            _cts?.Dispose();
        }
    }
}
