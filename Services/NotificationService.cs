using System;
using Booking.Services;

namespace Booking.Services
{
    public class NotificationService
    {
        private readonly ToastService _toastService;

        public NotificationService(ToastService toastService)
        {
            _toastService = toastService;
        }

        public void ShowNotification(string message, NotificationType type = NotificationType.Info, int duration = 5000)
        {
            switch (type)
            {
                case NotificationType.Success:
                    _toastService.ShowSuccess(message);
                    break;
                case NotificationType.Error:
                    _toastService.ShowError(message);
                    break;
                case NotificationType.Warning:
                    _toastService.ShowWarning(message);
                    break;
                default:
                    _toastService.ShowInfo(message);
                    break;
            }
        }
    }

    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error
    }
}
