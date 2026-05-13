using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Booking.Services
{
    public class ToastService
    {
        public event Action OnShow;
        public List<ToastMessage> Toasts { get; private set; } = new();

        public void ShowError(string message) => ShowToast(message, "error");
        public void ShowSuccess(string message) => ShowToast(message, "success");

        private void ShowToast(string message, string type)
        {
            var toast = new ToastMessage { Message = message, Type = type };
            Toasts.Add(toast);
            OnShow?.Invoke();

            Task.Delay(5000).ContinueWith(_ =>
            {
                Toasts.Remove(toast);
                OnShow?.Invoke();
            });
        }
    }

    public class ToastMessage
    {
        public string Message { get; set; } = "";
        public string Type { get; set; } = "error";
    }
}
