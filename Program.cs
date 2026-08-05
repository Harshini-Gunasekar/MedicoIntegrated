using Booking.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Booking.Services;
using Booking.Handlers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthenticationStateProvider>());

builder.Services.AddScoped<TenantHeaderHandler>();
builder.Services.AddScoped<UniIdentityRouteHandler>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpClient("DoctorApi", client => 
{
    var baseUrl = builder.Configuration["ApiBaseUrl"];
    if (string.IsNullOrEmpty(baseUrl)) throw new InvalidOperationException("ApiBaseUrl is not configured in appsettings.json");
    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddHttpClient("LabCareUrl", client => 
{
    var baseUrl = builder.Configuration["LabCareUrl"];
    if (string.IsNullOrEmpty(baseUrl)) throw new InvalidOperationException("LabCareUrl is not configured in appsettings.json");
    client.BaseAddress = new Uri(baseUrl);
})
.AddHttpMessageHandler<TenantHeaderHandler>()
.AddHttpMessageHandler<UniIdentityRouteHandler>();

builder.Services.AddHttpClient("RidoUrl", client => 
{
    var baseUrl = builder.Configuration["RidoUrl"];
    if (string.IsNullOrEmpty(baseUrl)) throw new InvalidOperationException("RidoUrl is not configured in appsettings.json");
    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddHttpClient("InventoryApi", client => 
{
    var baseUrl = builder.Configuration["ApiBaseUrl"];
    if (string.IsNullOrEmpty(baseUrl)) throw new InvalidOperationException("ApiBaseUrl is not configured in appsettings.json");
    if (!baseUrl.EndsWith("/"))
    {
        baseUrl += "/";
    }
    client.BaseAddress = new Uri(baseUrl + "api/");
});

builder.Services.AddHttpClient("Medico", client => 
{
    var baseUrl = builder.Configuration["ApiBaseUrl"];
    if (string.IsNullOrEmpty(baseUrl)) throw new InvalidOperationException("ApiBaseUrl is not configured in appsettings.json");
    if (!baseUrl.EndsWith("/")) baseUrl += "/";
    client.BaseAddress = new Uri(baseUrl);
})
.AddHttpMessageHandler<TenantHeaderHandler>()
.AddHttpMessageHandler<UniIdentityRouteHandler>();

builder.Services.AddHttpClient("MedicoAPI", client => 
{
    var baseUrl = builder.Configuration["ApiBaseUrl"];
    if (string.IsNullOrEmpty(baseUrl)) throw new InvalidOperationException("ApiBaseUrl is not configured in appsettings.json");
    if (!baseUrl.EndsWith("/")) baseUrl += "/";
    client.BaseAddress = new Uri(baseUrl);
})
.AddHttpMessageHandler<TenantHeaderHandler>()
.AddHttpMessageHandler<UniIdentityRouteHandler>();

builder.Services.AddScoped(sp => 
{
    var tenantHandler = sp.GetRequiredService<TenantHeaderHandler>();
    var routeHandler = sp.GetRequiredService<UniIdentityRouteHandler>();
    
    var httpClientHandler = new HttpClientHandler();
    routeHandler.InnerHandler = httpClientHandler;
    tenantHandler.InnerHandler = routeHandler;
    
    var client = new HttpClient(tenantHandler, disposeHandler: false);
    var baseUrl = builder.Configuration["ApiBaseUrl"];
    if (string.IsNullOrEmpty(baseUrl)) throw new InvalidOperationException("ApiBaseUrl is not configured in appsettings.json");
    client.BaseAddress = new Uri(baseUrl);
    return client;
});
builder.Services.AddScoped<Booking.Services.DoctorService>();
builder.Services.AddScoped<Booking.Services.DoctorProfileService>();
builder.Services.AddScoped<Booking.Services.LabResultService>();
builder.Services.AddScoped<Booking.Services.SlotService>();
builder.Services.AddScoped<Booking.Services.OpChargeSlabService>();
builder.Services.AddScoped<Booking.Services.DoctorAppointmentSlotTypeService>();
builder.Services.AddScoped<Booking.Services.DoctorTypeMasterService>();
builder.Services.AddScoped<Booking.Services.DoctorSpecialtyMasterService>();
builder.Services.AddScoped<Booking.Services.DoctorGroupMasterService>();
builder.Services.AddScoped<Booking.Services.ReimbursementCompanyMasterService>();
builder.Services.AddScoped<Booking.Services.ReportMethodService>();
builder.Services.AddScoped<Booking.Services.UomMasterService>();
builder.Services.AddScoped<Booking.Services.GroupMasterService>();
builder.Services.AddScoped<Booking.Services.PaymodeMasterService>();
builder.Services.AddScoped<Booking.Services.SampleMasterService>();
builder.Services.AddScoped<Booking.Services.TestTypeMasterService>();
builder.Services.AddScoped<Booking.Services.TestMasterService>();
builder.Services.AddScoped<Booking.Services.ToastService>();
builder.Services.AddScoped<Booking.Services.NotificationService>();
builder.Services.AddScoped<Booking.Services.AppointmentBookingService>();
builder.Services.AddScoped<Booking.Services.CaseSheetService>();
builder.Services.AddScoped<Booking.Services.VitalsService>();
builder.Services.AddScoped<Booking.Services.HmsBillingService>();
builder.Services.AddScoped<Booking.Services.HmsDueCollectionService>();
builder.Services.AddScoped<Booking.Services.CounterMasterService>();
builder.Services.AddScoped<Booking.Services.FeetypeService>();
builder.Services.AddScoped<Booking.Services.TestFeeService>();
builder.Services.AddScoped<Booking.Services.AreaMasterService>();
builder.Services.AddScoped<Booking.Services.CountryMasterService>();
builder.Services.AddScoped<Booking.Services.StateMasterService>();
builder.Services.AddScoped<Booking.Services.CityMasterService>();
builder.Services.AddScoped<Booking.Services.MasterTenantServices>();
builder.Services.AddScoped<SharedComponents.Rcl.Services.NotificationService>();
builder.Services.AddScoped<SharedComponents.Rcl.Services.TenantSessionState>();
builder.Services.AddScoped<Booking.Services.RoomTypeMasterService>();
builder.Services.AddScoped<Booking.Services.WardMasterService>();
builder.Services.AddScoped<Booking.Services.BedMasterService>();
builder.Services.AddScoped<Booking.Services.BedTransferService>();
builder.Services.AddScoped<Booking.Services.BedStatusService>();
builder.Services.AddScoped<Booking.Services.NurseMasterService>();
builder.Services.AddScoped<Booking.Services.BlockMasterService>();
builder.Services.AddScoped<Booking.Services.IpRegistrationService>();
builder.Services.AddScoped<Booking.Services.DischargeSummaryService>();
builder.Services.AddScoped<Booking.Services.LabSettingService>();
builder.Services.AddScoped<Booking.Services.FloorMasterService>();
builder.Services.AddScoped<Booking.Services.OGScreenService>();
builder.Services.AddScoped<Booking.Services.DashboardService>();
builder.Services.AddScoped<Booking.Services.AppointmentPreBookingService>();
builder.Services.AddScoped<Booking.Services.DoctorCurrentStatusService>();
builder.Services.AddScoped<MedicoAi.Services.UserSessionState>();
builder.Services.AddScoped<MedicoAi.Services.MedicoApiService>();
builder.Services.AddScoped<MedicoAi.Services.VitalsSignalRService>();
builder.Services.AddScoped<MedicoAi.Services.OllamaAiService>();
builder.Services.AddScoped<LabCare.Services.TestService>(sp =>
{
    var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient("DoctorApi");
    var session = sp.GetRequiredService<SharedComponents.Rcl.Services.TenantSessionState>();
    return new LabCare.Services.TestService(client, session);
});
builder.Services.AddScoped<LabCare.Services.GroupService>(sp =>
{
    var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient("LabCareUrl");
    var session = sp.GetRequiredService<SharedComponents.Rcl.Services.TenantSessionState>();
    return new LabCare.Services.GroupService(client, session);
});
builder.Services.AddScoped<LabCare.Services.UserRightsService>(sp =>
{
    var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient("LabCareUrl");
    var session = sp.GetRequiredService<SharedComponents.Rcl.Services.TenantSessionState>();
    return new LabCare.Services.UserRightsService(client, session);
});
builder.Services.AddScoped<LabCare.Services.UserService>(sp =>
{
    var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient("LabCareUrl");
    var session = sp.GetRequiredService<SharedComponents.Rcl.Services.TenantSessionState>();
    return new LabCare.Services.UserService(client, session);
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
