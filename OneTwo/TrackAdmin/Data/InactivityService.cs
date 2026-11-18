using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace TrackAdmin.Data
{
    public class InactivityService : IDisposable
    {
        private readonly NavigationManager _navigationManager;
        private readonly IJSRuntime _jsRuntime;
        private System.Timers.Timer _timer;
        private const int TimeoutMinutes = 60;

        public InactivityService(NavigationManager navigationManager, IJSRuntime jsRuntime)
        {
            _navigationManager = navigationManager;
            _jsRuntime = jsRuntime;
            
            _timer = new System.Timers.Timer(60000); // Check every minute
            _timer.Elapsed += OnTimerElapsed;
        }

        private DateTime _lastActivity = DateTime.UtcNow;

        public void RecordActivity()
        {
            _lastActivity = DateTime.UtcNow;
            
            if (!_timer.Enabled)
            {
                _timer.Start();
            }
        }

        [JSInvokable]
        public void OnActivityDetected()
        {
            RecordActivity();
        }

        private void OnTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (DateTime.UtcNow - _lastActivity > TimeSpan.FromMinutes(TimeoutMinutes))
            {
                _timer.Stop();
                Logout();
            }
        }


        private void Logout()
        {
            _navigationManager.NavigateTo("/", true);
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
