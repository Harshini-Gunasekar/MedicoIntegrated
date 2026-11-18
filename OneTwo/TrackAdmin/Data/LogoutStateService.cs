namespace TrackAdmin.Data
{
    /// <summary>
    /// Shared service that NavMenu / RightTop can use to signal a logout,
    /// while MainLayout renders the beautiful goodbye overlay.
    /// </summary>
    public class LogoutStateService
    {
        public event Func<Task>? OnLogoutRequested;

        public async Task RequestLogoutAsync()
        {
            if (OnLogoutRequested != null)
                await OnLogoutRequested.Invoke();
        }
    }
}
