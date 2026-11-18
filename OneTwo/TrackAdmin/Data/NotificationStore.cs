using System;
using System.Collections.Generic;

namespace TrackAdmin.Data
{
    /// <summary>
    /// Singleton in-memory store for admin bell-panel notifications.
    /// Survives Blazor page navigation (component dispose/recreate cycles).
    /// Cleared only when the admin explicitly clicks "Clear All".
    /// </summary>
    public class NotificationStore
    {
        private readonly List<NotifEntry> _items = new();

        public IReadOnlyList<NotifEntry> Items => _items.AsReadOnly();

        public int UnreadCount => _items.Count;

        public event Action? OnChange;
        public event Action? OnShowHistoryRequested;

        public void RequestShowHistory() => OnShowHistoryRequested?.Invoke();

        public void Add(string title, string sub, string type, string icon, string photoUrl = "")
        {
            // Payment request ID deduplication: Keep only the latest notification for any given payment request ID
            if ((title.Contains("Payment", StringComparison.OrdinalIgnoreCase) ||
                 sub.Contains("Payment", StringComparison.OrdinalIgnoreCase) ||
                 title.Contains("Request", StringComparison.OrdinalIgnoreCase) ||
                 sub.Contains("Request", StringComparison.OrdinalIgnoreCase)) &&
                 !title.Contains("Booking", StringComparison.OrdinalIgnoreCase) &&
                 !sub.Contains("Booking", StringComparison.OrdinalIgnoreCase))
            {
                int hashIdx = sub.IndexOf('#');
                if (hashIdx == -1) hashIdx = title.IndexOf('#');

                if (hashIdx != -1)
                {
                    int len = 0;
                    while (hashIdx + 1 + len < sub.Length && char.IsDigit(sub[hashIdx + 1 + len]))
                    {
                        len++;
                    }
                    if (len == 0)
                    {
                        hashIdx = title.IndexOf('#');
                        len = 0;
                        while (hashIdx + 1 + len < title.Length && char.IsDigit(title[hashIdx + 1 + len]))
                        {
                            len++;
                        }
                    }

                    if (len > 0)
                    {
                        string reqIdStr = (hashIdx + 1 + len <= sub.Length) ? sub.Substring(hashIdx + 1, len) : title.Substring(hashIdx + 1, len);
                        _items.RemoveAll(x =>
                            (x.Title.Contains("Payment", StringComparison.OrdinalIgnoreCase) ||
                             x.Sub.Contains("Payment", StringComparison.OrdinalIgnoreCase) ||
                             x.Title.Contains("Request", StringComparison.OrdinalIgnoreCase) ||
                             x.Sub.Contains("Request", StringComparison.OrdinalIgnoreCase)) &&
                            (x.Title.Contains("#" + reqIdStr) || x.Sub.Contains("#" + reqIdStr))
                        );
                    }
                }
            }

            // Deduplicate: If there is already an entry with the same Title and Sub within the last 10 seconds, ignore it
            var duplicate = _items.Find(x => x.Title == title && x.Sub == sub && (DateTime.Now - x.Time).TotalSeconds < 10);
            if (duplicate != null)
            {
                // Just update the time to keep it fresh, but don't add a duplicate entry
                duplicate.Time = DateTime.Now;
                OnChange?.Invoke();
                return;
            }

            _items.Insert(0, new NotifEntry
            {
                Title    = title,
                Sub      = sub,
                Type     = type,
                Icon     = icon,
                PhotoUrl = photoUrl,
                Time     = DateTime.Now
            });

            // Cap at 50 entries to avoid memory growth
            if (_items.Count > 50)
                _items.RemoveAt(_items.Count - 1);

            OnChange?.Invoke();
        }

        public void Clear()
        {
            _items.Clear();
            OnChange?.Invoke();
        }
    }

    public class NotifEntry
    {
        public string Title    { get; set; } = "";
        public string Sub      { get; set; } = "";
        public string Type     { get; set; } = "info";
        public string Icon     { get; set; } = "bi-bell-fill";
        public string PhotoUrl { get; set; } = "";
        public DateTime Time   { get; set; } = DateTime.Now;
    }
}
