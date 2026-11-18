using System;
using System.Collections.Generic;

namespace LabCare.Services
{
    public class NotificationService
    {
        public event Action<string, NotificationType>? OnShow;
        public event Action<ToastMessage>? OnShowToast;
        public event Action? OnToastsChanged;

        private readonly List<ToastMessage> _toasts = new();
        public IReadOnlyList<ToastMessage> Toasts => _toasts;

        public void Show(string message, NotificationType type = NotificationType.Info)
        {
            Show(type.ToString(), message, type);
        }

        public void Show(string title, string message, NotificationType type = NotificationType.Info, int duration = 5000)
        {
            var toast = new ToastMessage
            {
                Title = title,
                Message = message,
                Type = type,
                Duration = duration
            };

            _toasts.Add(toast);
            OnToastsChanged?.Invoke();

            OnShow?.Invoke(message, type);
            OnShowToast?.Invoke(toast);
        }

        public void Clear(string id)
        {
            var toast = _toasts.Find(t => t.Id == id);
            if (toast != null)
            {
                _toasts.Remove(toast);
                OnToastsChanged?.Invoke();
            }
        }

        public void ClearAll()
        {
            _toasts.Clear();
            OnToastsChanged?.Invoke();
        }

        public void Success(string message) => Show("Success", message, NotificationType.Success);
        public void Success(string title, string message, int duration = 5000) => Show(title, message, NotificationType.Success, duration);

        public void Error(string message) => Show("Error", message, NotificationType.Error);
        public void Error(string title, string message, int duration = 5000) => Show(title, message, NotificationType.Error, duration);

        public void Warning(string message) => Show("Warning", message, NotificationType.Warning);
        public void Warning(string title, string message, int duration = 5000) => Show(title, message, NotificationType.Warning, duration);

        public void Info(string message) => Show("Info", message, NotificationType.Info);
        public void Info(string title, string message, int duration = 5000) => Show(title, message, NotificationType.Info, duration);
    }

    public class ToastMessage
    {
        public string Id { get; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; } = NotificationType.Info;
        public int Duration { get; set; } = 5000; // in milliseconds
        public DateTime Timestamp { get; } = DateTime.UtcNow;
    }

    public enum NotificationType
    {
        Success,
        Error,
        Warning,
        Info
    }
}
