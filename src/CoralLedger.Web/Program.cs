using CoralLedger.Application;
using CoralLedger.Application.Common.Interfaces;
using CoralLedger.Infrastructure;
using CoralLedger.Infrastructure.Data;
using CoralLedger.Infrastructure.Data.Seeding;
using CoralLedger.Web.Components;
using CoralLedger.Web.Endpoints;
using CoralLedger.Web.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add Blazor components with Interactive Server mode + WebAssembly Auto mode
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// Add Application layer (MediatR, FluentValidation)
builder.Services.AddApplication();

// Add Infrastructure layer services (including external API clients)
builder.Services.AddInfrastructure(builder.Configuration);

// Add background job scheduler (Quartz.NET)
builder.Services.AddQuartzJobs();

// Add SignalR for real-time notifications
builder.Services.AddSignalR();
builder.Services.AddScoped<IAlertHubContext, AlertHubContext>();

// Add Database with PostGIS support (skip in testing environment - tests configure their own)
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.AddMarineDatabase("marinedb");
}

var app = builder.Build();

// Initialize and seed database (skip in testing environment)
if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<MarineDbContext>();

    // Ensure database is created and apply any pending migrations
    await context.Database.EnsureCreatedAsync();

    // Seed the database with Bahamas MPA data
    await BahamasMpaSeeder.SeedAsync(context);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

// Serve WebAssembly files
app.UseBlazorFrameworkFiles();
app.MapStaticAssets();

// Map API endpoints
app.MapMpaEndpoints();
app.MapVesselEndpoints();
app.MapBleachingEndpoints();
app.MapJobEndpoints();
app.MapObservationEndpoints();
app.MapAIEndpoints();
app.MapAlertEndpoints();
app.MapAisEndpoints();
app.MapExportEndpoints();
app.MapAdminEndpoints();

// Map SignalR hub
app.MapHub<AlertHub>("/hubs/alerts");

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(CoralLedger.Web.Client._Imports).Assembly);

app.MapDefaultEndpoints();

app.Run();

// Make Program accessible for integration testing
public partial class Program { }
