namespace TrackAdmin.Data
{
    public class PageMetadataService
    {
        public string Title { get; set; } = "Dashboard";
        public string Subtitle { get; set; } = "Mission Control Overview";
        public string Icon { get; set; } = "bi-grid-fill";
        public List<PageStat> Stats { get; set; } = new List<PageStat>();

        public event Action OnChange;

        public void UpdatePage(string title, string subtitle, string icon, List<PageStat> stats = null)
        {
            Title = title;
            Subtitle = subtitle;
            Icon = icon;
            Stats = stats ?? new List<PageStat>();
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }

    public class PageStat
    {
        public string Label { get; set; }
        public string Value { get; set; }
        public string Color { get; set; } = "text-primary";
    }
}
