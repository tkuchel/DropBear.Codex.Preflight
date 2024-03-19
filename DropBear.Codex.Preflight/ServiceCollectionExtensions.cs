using DropBear.Codex.Preflight.Interfaces;
using DropBear.Codex.Preflight.Models;
using DropBear.Codex.Preflight.Services;
using MessagePipe;
using Microsoft.Extensions.DependencyInjection;

namespace DropBear.Codex.Preflight;

public static class PreflightCheckServiceExtensions
{
    /// <summary>
    /// Adds services required for preflight checks to the specified IServiceCollection.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <returns>The IServiceCollection so that additional calls can be chained.</returns>
    public static IServiceCollection AddPreflightChecks(this IServiceCollection services)
    {
        // Ensure all necessary interfaces and their implementations are registered
        services.AddSingleton<MainPreflightManager>();
        services.AddSingleton<IPreflightSubManager, PreflightSubManager>();
        services.AddSingleton<IPreflightTask, PreflightTask>();

        // Add MessagePipe with default configuration or customize as needed
        services.AddMessagePipe(options =>
        {
            // Customize MessagePipe options if necessary
        });

        // Note: If ZLogger is being used or any other logging, ensure it's configured appropriately here
        // services.AddZLoggerConsole();

        return services;
    }
}