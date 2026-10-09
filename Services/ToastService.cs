using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Booking.Services
{
    public class ToastService
    {
        private readonly object _lock = new();
        private readonly List<ToastMessage> _toasts = new();

        public event Action? OnShow;

        public List<ToastMessage> Toasts
        {
            get
            {
                lock (_lock)
                {
                    return _toasts.ToList();
                }
            }
        }

        public void ShowError(string message) => ShowToast(message, "error");
        public void ShowSuccess(string message) => ShowToast(message, "success");
        public void ShowWarning(string message) => ShowToast(message, "warning");
        public void ShowInfo(string message) => ShowToast(message, "info");

        public void Success(string message) => ShowSuccess(message);
        public void Error(string message) => ShowError(message);
        public void Warning(string message) => ShowWarning(message);
        public void Info(string message) => ShowInfo(message);

        public void Remove(ToastMessage toast) => RemoveToast(toast);

        public void RemoveToast(ToastMessage toast)
        {
            if (toast == null) return;
            bool removed = false;
            lock (_lock)
            {
                removed = _toasts.Remove(toast);
                if (!removed)
                {
                    var match = _toasts.FirstOrDefault(t => t.Id == toast.Id);
                    if (match != null)
                    {
                        removed = _toasts.Remove(match);
                    }
                }
            }

            if (removed)
            {
                OnShow?.Invoke();
            }
        }

        private void ShowToast(string message, string type)
        {
            if (string.IsNullOrWhiteSpace(message)) return;

            var toast = new ToastMessage { Message = message, Type = type };
            lock (_lock)
            {
                _toasts.Add(toast);
            }
            OnShow?.Invoke();

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(5000);
                    RemoveToast(toast);
                }
                catch { }
            });
        }
    }

    public class ToastMessage
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Message { get; set; } = "";
        public string Type { get; set; } = "error";
    }
}
