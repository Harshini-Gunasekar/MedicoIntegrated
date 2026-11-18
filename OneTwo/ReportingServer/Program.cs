using ReportingServer.Component;
using ReportingServer.Controllers;
using ReportingServer.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents();
builder.Services.AddControllers();
builder.Services.AddScoped<BillReceipt>();
builder.Services.AddScoped<RoutineReportClass>();
builder.Services.AddScoped<CultureClass>();
builder.Services.AddScoped<ReceiptClass>();
builder.Services.AddScoped<StatementClass>();
builder.Services.AddScoped<SummaryClass>();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseAntiforgery();
app.UseAuthorization();
app.MapRazorComponents<App>();
app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI();
app.Run();
