using SalesforceManager.Services.Salesforce;
using SalesforceManager.Services.Salesforce.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.Configure<SalesforceConfig>(
    builder.Configuration.GetSection("Salesforce"));

builder.Services.AddHttpClient<SalesforceApiClient>();
builder.Services.AddScoped<SalesforceService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
