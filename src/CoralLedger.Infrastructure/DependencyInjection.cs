using CoralLedger.Application.Common.Interfaces;
using CoralLedger.Infrastructure.Data;
using CoralLedger.Infrastructure.ExternalServices;
using CoralLedger.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CoralLedger.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IDateTimeService, DateTimeService>();

        // Register Global Fishing Watch client
        services.Configure<GlobalFishingWatchOptions>(
            configuration.GetSection(GlobalFishingWatchOptions.SectionName));

        services.AddHttpClient<IGlobalFishingWatchClient, GlobalFishingWatchClient>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(60);
        });

        // Register NOAA Coral Reef Watch client
        services.AddHttpClient<ICoralReefWatchClient, CoralReefWatchClient>(client =>
        {
            client.Timeout = TimeSpan.FromMinutes(5);
        });

        return services;
    }

    public static void AddMarineDatabase(
        this IHostApplicationBuilder builder,
        string connectionName)
    {
        // Aspire's AddNpgsqlDbContext handles the connection, but we need to configure NTS
        // Using the configureDbContextOptions callback to add NetTopologySuite support
        builder.AddNpgsqlDbContext<MarineDbContext>(connectionName,
            configureDbContextOptions: options =>
            {
                // Configure Npgsql to use NetTopologySuite for spatial types
                // Note: This is a workaround as Aspire's API doesn't expose configureDataSourceBuilder
                options.UseNpgsql(npgsqlOptions =>
                {
                    npgsqlOptions.UseNetTopologySuite();
                });
            });

        builder.Services.AddScoped<IMarineDbContext>(sp =>
            sp.GetRequiredService<MarineDbContext>());
    }
}
