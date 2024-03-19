using DropBear.Codex.Preflight.Interfaces;
using DropBear.Codex.Preflight.Models;
using DropBear.Codex.Preflight.Services;
using MessagePipe;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace DropBear.Codex.Preflight;

public static class PreflightCheckServiceExtensions
{
    public static IServiceCollection AddPreflightChecks(this IServiceCollection services)
    {
        services.AddSingleton<MainPreflightManager>();
        services.AddSingleton<IPreflightSubManager, PreflightSubManager>();
        services.AddSingleton<IPreflightTask, PreflightTask>();

        services.AddMessagePipe(options =>
        {
            // Customize MessagePipe options if necessary
        });

        // Check if an ILogger is already registered and only add ZLogger if not
        if (services.All(x => x.ServiceType != typeof(ILogger)))
            // Example of adding ZLogger assuming it provides or works with Microsoft.Extensions.Logging.ILogger
            // Adjust this as necessary based on your setup and ZLogger configuration.
            services.AddLogging(builder =>
            {
                builder.ClearProviders(); // Optional: Clear existing providers
                builder.AddZLoggerConsole();
            });

        AddLoggingAdapter(services);

        return services;
    }

    private static IServiceCollection AddLoggingAdapter(this IServiceCollection services)
    {
        // Assuming `AddPreflightChecks` is already called and potentially registers ZLogger or another ILogger.
        services.AddSingleton(typeof(IAppLogger<>), typeof(AppLogger<>));
        return services;
    }
}