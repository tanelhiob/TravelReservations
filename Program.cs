using Microsoft.EntityFrameworkCore;
using TravelReservations.Data;

var builder = WebApplication.CreateBuilder(args);
// Read DetailedErrors setting from configuration (appsettings.json / appsettings.Development.json)
var detailedErrors = builder.Configuration.GetValue<bool>("DetailedErrors");

// Add services to the container.
builder.Services.AddRazorPages();
// Configure server-side Blazor with detailed errors if enabled
builder.Services.AddServerSideBlazor()
    .AddCircuitOptions(options => options.DetailedErrors = detailedErrors);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("TravelDb"));
// Register our route search service
builder.Services.AddScoped<RouteSearchService>();
// Register background service for pricelist updates
builder.Services.AddHostedService<PriceListBackgroundService>();

// HttpClient for TravelPrices API
builder.Services.AddHttpClient("TravelApi", client =>
{
    client.BaseAddress = new Uri("https://cosmosodyssey.azurewebsites.net/api/v1.0/");
});

var app = builder.Build();

// Initialize in-memory data from API
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await DataInitializer.InitializeAsync(services);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Map Blazor hub
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.MapRazorPages();

app.Run();