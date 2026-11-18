using System;
using System.Collections.Generic;
using System.Timers;

namespace Inventory_management_frontend.Services
{
    public class NotificationService
    {
        public event Action? OnChange;
        private List<NotificationMessage> _notifications = new();

        public List<NotificationMessage> GetNotifications() => _notifications;

        public void ShowNotification(string message, NotificationType type = NotificationType.Info, int duration = 5000)
        {
            var notification = new NotificationMessage
            {
                Id = Guid.NewGuid(),
                Message = message,
                Type = type,
                CreatedAt = DateTime.Now
            };

            _notifications.Add(notification);
            NotifyStateChanged();

            var timer = new System.Timers.Timer(duration);
            timer.Elapsed += (s, e) => 
            {
                RemoveNotification(notification.Id);
                timer.Dispose();
            };
            timer.AutoReset = false;
            timer.Start();
        }

        public void RemoveNotification(Guid id)
        {
            _notifications.RemoveAll(n => n.Id == id);
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }

    public class NotificationMessage
    {
        public Guid Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error
    }
}
