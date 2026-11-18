using LabCare.Components;
using LabCare.Components.Data;
using LabCare.Handlers;
using LabCare.Services;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers();

builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new LabCare.Models.AppSettings
    {
        BaseUrl = config["BaseUrl"] ?? string.Empty
    };
});

builder.Services.AddSingleton<SharedComponents.Rcl.Models.AppSettings>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new SharedComponents.Rcl.Models.AppSettings
    {
        BaseUrl = config["BaseUrl"] ?? string.Empty
    };
});
// Register IOptions<SharedComponents.Rcl.Models.AppSettings> for shared master components
builder.Services.Configure<SharedComponents.Rcl.Models.AppSettings>(opts =>
{
    opts.BaseUrl = builder.Configuration["BaseUrl"] ?? string.Empty;
});

builder.Services.AddScoped<UserSessionService>();
builder.Services.AddScoped<TenantSessionState>();
// Forward LabCare.Services.TenantSessionState -> SharedComponents version for shared master components
builder.Services.AddScoped<SharedComponents.Rcl.Services.TenantSessionState>(sp =>
{
    var lc = sp.GetRequiredService<TenantSessionState>();
    var shared = new SharedComponents.Rcl.Services.TenantSessionState();
    // Propagate current session if already set
    if (lc.IsAuthenticated)
        shared.SetSession(lc.TenantCode, lc.AuthToken, lc.TenantName, lc.UserCode, lc.BranchCode, lc.CounterCode);
    return shared;
});
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<SharedComponents.Rcl.Services.NotificationService>();
builder.Services.AddScoped<BillingService>();
builder.Services.AddScoped<ExecutionClass>();
builder.Services.AddScoped<TestService>();
builder.Services.AddScoped<CounterService>();
builder.Services.AddScoped<GroupService>();
builder.Services.AddScoped<CashVoucherService>();
builder.Services.AddScoped<SampleCollectionService>();
builder.Services.AddScoped<UserRightsService>();

// ─── Tenant Header Handler (DelegatingHandler) ───────────────────────────────
// Registered as Transient because DelegatingHandlers must be transient.
builder.Services.AddTransient<TenantHeaderHandler>();

builder.Services.AddHttpClient("LabCareApi", (sp, client) =>
{
    var settings = sp.GetRequiredService<LabCare.Models.AppSettings>();
    client.BaseAddress = new Uri(settings.BaseUrl);
})
.AddHttpMessageHandler<TenantHeaderHandler>();

// ─── Default injected HttpClient (for components that use @inject HttpClient) ─
builder.Services.AddScoped(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return factory.CreateClient("LabCareApi");
});

// ─────────────────────────────────────────────────────────────────────────────
// Authentication Services
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
    provider.GetRequiredService<JwtAuthenticationStateProvider>());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // app.UseWebAssemblyDebugging(); (Removed since Client project is missing)
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

// Middleware to collapse double slashes (e.g. //sso-login -> /sso-login)
app.Use((context, next) =>
{
    var path = context.Request.Path.Value;
    if (path != null && path.StartsWith("//"))
    {
        // Only fix the start of the path to avoid breaking internal framework slashes
        context.Request.Path = "/" + path.TrimStart('/');
    }
    return next();
});

app.UseStaticFiles();
app.UseWebSockets(); 
app.UseAntiforgery();

app.MapStaticAssets();
app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(SharedComponents.Rcl.Components.Register).Assembly);

app.Run();
