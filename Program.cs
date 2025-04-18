using Microsoft.EntityFrameworkCore;
using TravelReservations.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("TravelDb"));

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

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.MapRazorPages();

app.Run();