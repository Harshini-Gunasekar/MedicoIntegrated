using System;

namespace SharedComponents.Rcl.Services
{
    public class NotificationService
    {
        public event Action<string, NotificationType>? OnShow;

        public void Show(string message, NotificationType type = NotificationType.Info)
        {
            OnShow?.Invoke(message, type);
        }

        public void Success(string message) => Show(message, NotificationType.Success);
        public void Error(string message) => Show(message, NotificationType.Error);
        public void Warning(string message) => Show(message, NotificationType.Warning);
        public void Info(string message) => Show(message, NotificationType.Info);
    }

    public enum NotificationType
    {
        Success,
        Error,
        Warning,
        Info
    }
}
