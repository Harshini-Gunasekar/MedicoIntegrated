using Booking.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient("DoctorApi", client => 
{
    var baseUrl = builder.Configuration["ApiBaseUrl"];
    if (string.IsNullOrEmpty(baseUrl)) throw new InvalidOperationException("ApiBaseUrl is not configured in appsettings.json");
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("tenant_code", "TEN1011");
});

builder.Services.AddHttpClient("LabCareUrl", client => 
{
    var baseUrl = builder.Configuration["LabCareUrl"];
    if (string.IsNullOrEmpty(baseUrl)) throw new InvalidOperationException("LabCareUrl is not configured in appsettings.json");
    client.BaseAddress = new Uri(baseUrl);

});

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("DoctorApi"));
builder.Services.AddScoped<Booking.Services.DoctorService>();
builder.Services.AddScoped<Booking.Services.SlotService>();
builder.Services.AddScoped<Booking.Services.DoctorAppointmentSlotTypeService>();
builder.Services.AddScoped<Booking.Services.DoctorTypeMasterService>();
builder.Services.AddScoped<Booking.Services.DoctorSpecialtyMasterService>();
builder.Services.AddScoped<Booking.Services.ReimbursementCompanyMasterService>();
builder.Services.AddScoped<Booking.Services.ReportMethodService>();
builder.Services.AddScoped<Booking.Services.UomMasterService>();
builder.Services.AddScoped<Booking.Services.GroupMasterService>();
builder.Services.AddScoped<Booking.Services.SampleMasterService>();
builder.Services.AddScoped<Booking.Services.TestTypeMasterService>();
builder.Services.AddScoped<Booking.Services.TestMasterService>();
builder.Services.AddScoped<Booking.Services.ToastService>();
builder.Services.AddScoped<Booking.Services.AppointmentBookingService>();
builder.Services.AddScoped<Booking.Services.CaseSheetService>();
builder.Services.AddScoped<Booking.Services.HmsBillingService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
