namespace TrackAdmin.Data
{
    public class UserSessionService
    {
        public bool IsLoggedIn { get; set; } = false;
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string TenantID { get; set; }
        public bool IsAdmin { get; set; }
        public bool WelcomeCardShown { get; set; } = false;
        private SharedComponents.Rcl.Models.UserRights _rights = new();
        public SharedComponents.Rcl.Models.UserRights Rights 
        { 
            get => _rights; 
            set => _rights = value ?? new SharedComponents.Rcl.Models.UserRights(); 
        }

        public void Clear()
        {
            IsLoggedIn = false;
            UserID = 0;
            UserName = null;
            TenantID = null;
            IsAdmin = false;
            WelcomeCardShown = false;
            Rights = new SharedComponents.Rcl.Models.UserRights();
        }
    }
}