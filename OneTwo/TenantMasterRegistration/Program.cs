using TenantMasterRegistration.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register AppSettings and other services
builder.Services.AddHttpClient();
builder.Services.AddSingleton(builder.Configuration.GetSection("AppSettings").Get<SharedComponents.Rcl.Models.AppSettings>() ?? new SharedComponents.Rcl.Models.AppSettings());
builder.Services.AddScoped<SharedComponents.Rcl.Services.NotificationService>();

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
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(SharedComponents.Rcl.Components.Register).Assembly);

app.Run();
