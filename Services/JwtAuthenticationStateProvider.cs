using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Booking.Services
{
    public class JwtAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ProtectedSessionStorage _sessionStorage;
        private readonly ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        public JwtAuthenticationStateProvider(ProtectedSessionStorage sessionStorage)
        {
            _sessionStorage = sessionStorage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var result = await _sessionStorage.GetAsync<UserSession>("UserSession");
                var userSession = result.Success ? result.Value : null;
                SharedComponents.Rcl.Models.UserDetailsModel? userDetails = null;
                if (userSession == null || string.IsNullOrWhiteSpace(userSession.Token))
                {
                    var detailsResult = await _sessionStorage.GetAsync<SharedComponents.Rcl.Models.UserDetailsModel>("UserDetails");
                    userDetails = detailsResult.Success ? detailsResult.Value : null;
                }

                string? token = userSession?.Token ?? userDetails?.AuthToken;

                if (string.IsNullOrEmpty(token))
                {
                    var tokenResult = await _sessionStorage.GetAsync<string>("authToken");
                    token = tokenResult.Success ? tokenResult.Value : null;
                }

                if (string.IsNullOrWhiteSpace(token))
                {
                    return await Task.FromResult(new AuthenticationState(_anonymous));
                }

                var claims = new List<Claim>();
                claims.AddRange(ParseClaimsFromJwt(token));

                // Validate expiration
                var expClaim = claims.FirstOrDefault(c => c.Type == "exp");
                if (expClaim != null && long.TryParse(expClaim.Value, out var expTime))
                {
                    var expiryDate = DateTimeOffset.FromUnixTimeSeconds(expTime);
                    if (expiryDate < DateTimeOffset.UtcNow)
                    {
                        // Token expired
                        return await Task.FromResult(new AuthenticationState(_anonymous));
                    }
                }

                
                // Synthesize role and IsAdmin claims for compatibility between SSO and standard login
                if (!claims.Any(c => c.Type == ClaimTypes.Role))
                {
                    var roleClaim = claims.FirstOrDefault(c => c.Type == "role")?.Value ?? userDetails?.Role;
                    if (!string.IsNullOrEmpty(roleClaim))
                    {
                        claims.Add(new Claim(ClaimTypes.Role, roleClaim));
                    }
                }
                
                if (!claims.Any(c => c.Type == "role"))
                {
                    var roleClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? userDetails?.Role;
                    if (!string.IsNullOrEmpty(roleClaim))
                    {
                        claims.Add(new Claim("role", roleClaim));
                    }
                }

                // Ensure IsAdmin claim exists if user is Admin, Administrator, or Tenant
                var finalRole = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role")?.Value;
                if (string.Equals(finalRole, "Administrator", StringComparison.OrdinalIgnoreCase) || 
                    string.Equals(finalRole, "Admin", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(finalRole, "Tenant", StringComparison.OrdinalIgnoreCase))
                {
                    if (!claims.Any(c => c.Type == "IsAdmin"))
                    {
                        claims.Add(new Claim("IsAdmin", "true"));
                    }
                    // Map Tenant to Administrator role for downstream RCL components
                    if (!claims.Any(c => c.Type == ClaimTypes.Role && (c.Value == "Administrator" || c.Value == "Admin")))
                    {
                        claims.Add(new Claim(ClaimTypes.Role, "Administrator"));
                    }
                    if (!claims.Any(c => c.Type == "role" && (c.Value == "Administrator" || c.Value == "Admin")))
                    {
                        claims.Add(new Claim("role", "Administrator"));
                    }
                }

                var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "JwtAuth"));
                return await Task.FromResult(new AuthenticationState(claimsPrincipal));

            }
            
            catch
            {
                return await Task.FromResult(new AuthenticationState(_anonymous));
            }
        }

        public async Task UpdateAuthenticationState(UserSession? userSession)
        {
            ClaimsPrincipal claimsPrincipal;

            if (userSession != null && !string.IsNullOrWhiteSpace(userSession.Token))
            {
                await _sessionStorage.SetAsync("UserSession", userSession);
                claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(userSession.Token), "JwtAuth"));
            }
            else
            {
                await _sessionStorage.DeleteAsync("UserSession");
                claimsPrincipal = _anonymous;
            }

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
        }

        public async Task<UserSession?> GetUserSessionAsync()
        {
            var result = await _sessionStorage.GetAsync<UserSession>("UserSession");
            return result.Success ? result.Value : null;
        }

        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            var segments = jwt.Split('.');
            if (segments.Length < 2) return claims;

            var payload = segments[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);

            Dictionary<string, object>? keyValuePairs = null;
            try
            {
                keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
            }
            catch { }

            if (keyValuePairs != null)
            {
                foreach (var kvp in keyValuePairs)
                {
                    claims.Add(new Claim(kvp.Key, kvp.Value.ToString() ?? ""));
                }
            }

            return claims;
        }

        public UserSession CreateSessionFromToken(string token)
        {
            var claims = ParseClaimsFromJwt(token);
            var session = new UserSession { Token = token };

            session.UserID = claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "UserIdentifier")?.Value ?? "";
            session.GlobalUserID = claims.FirstOrDefault(c => c.Type == "GlobalUserId")?.Value ?? "";
            session.UserName = claims.FirstOrDefault(c => c.Type == "unique_name" || c.Type == "name")?.Value ?? "";
            session.TenantID = claims.FirstOrDefault(c => c.Type == "TenantId")?.Value ?? "";
            session.TenantName = claims.FirstOrDefault(c => c.Type == "TenantName")?.Value ?? "";

            return session;
        }

        private byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }

    public class UserSession
    {
        public string UserID { get; set; }
        public string GlobalUserID { get; set; }
        public string UserName { get; set; }
        public string TenantID { get; set; }
        public string TenantName { get; set; }
        public string Token { get; set; }
    }
}
