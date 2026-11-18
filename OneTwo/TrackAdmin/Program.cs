using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.Options;
using Syncfusion.Blazor;
using Syncfusion.Blazor.Popups;
using TrackAdmin.Components;
using TrackAdmin.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddHubOptions(options =>
    {
        // Raise SignalR message size limit so large files (photos, voice recordings) 
        // can stream through Blazor's JS interop and InputFile without truncation.
        // Default is 32KB which breaks uploads > 1MB and voice recordings > ~2 seconds.
        options.MaximumReceiveMessageSize = 50 * 1024 * 1024; // 50 MB
    });

builder.Services.AddSyncfusionBlazor();
builder.Services.AddScoped<SfDialogService>();
builder.Services.AddScoped<MessageService>();
builder.Services.Configure<APIConfiguration>(builder.Configuration.GetSection("ApiSettings"));
builder.Services.AddSingleton<IAPIConfiguration, APIConfiguration>();

// Register SharedComponents.Rcl services
builder.Services.AddSingleton(new SharedComponents.Rcl.Models.AppSettings {
    BaseUrl = builder.Configuration.GetValue<string>("AppSettings:BaseUrl") ?? "https://uniidentityapi.iscansoft.com/"
});
builder.Services.AddScoped<SharedComponents.Rcl.Services.NotificationService>();
builder.Services.AddScoped<SharedComponents.Rcl.Services.TenantSessionState>();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthenticationStateProvider>());

builder.Services.AddScoped<RiderTracking>();
builder.Services.AddScoped<InactivityService>();

builder.Services.AddScoped<MasterData>();

builder.Services.AddScoped<TenantState>();
builder.Services.AddScoped<TenantHttpService>();
builder.Services.AddScoped<TenantService>();
builder.Services.AddScoped<TenantHeaderHandler>();
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<UserSessionService>();
builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddScoped<PageMetadataService>();
builder.Services.AddScoped<LogoutStateService>();
builder.Services.AddSingleton<NotificationStore>(); // Persists bell-panel notifications across page navigation

// Register HttpClient with the custom handler directly (Reliable for Blazor Server)
builder.Services.AddScoped<HttpClient>(sp =>
{
    var apiConfig = sp.GetRequiredService<IOptions<APIConfiguration>>().Value;
    var tenantState = sp.GetRequiredService<TenantState>();
    var sessionStorage = sp.GetRequiredService<ProtectedSessionStorage>();

    var handler = new TenantHeaderHandler(tenantState, sessionStorage)
    {
        InnerHandler = new HttpClientHandler()
    };

    return new HttpClient(handler)
    {
        BaseAddress = new Uri(apiConfig.BaseApiUrl)
    };
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie();



builder.Services.AddAuthorization();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mzc3OTk4NkAzMjM4MmUzMDJlMzBWWnYxRitUN2lubXpiK1RzdmZNV2ZSbWcyUUdZK1FMenJBWDVRazV5V2hJPQ==");

app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.MapStaticAssets();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(SharedComponents.Rcl.Components.Register).Assembly);

app.Run();