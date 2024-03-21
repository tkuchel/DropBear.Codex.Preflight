using DropBear.Codex.AppLogger.Extensions;
using DropBear.Codex.Preflight.Interfaces;
using DropBear.Codex.Preflight.Services;
using MessagePipe;
using Microsoft.Extensions.DependencyInjection;

namespace DropBear.Codex.Preflight;

/// <summary>
///     Extension methods for configuring preflight check services in the dependency injection container.
/// </summary>
// ReSharper disable once UnusedType.Global
public static class PreflightCheckServiceExtensions
{
    /// <summary>
    ///     Adds preflight check services to the specified <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add the services to.</param>
    /// <returns>The <see cref="IServiceCollection" /> so that additional calls can be chained.</returns>
    // ReSharper disable once UnusedMember.Global
    public static IServiceCollection AddPreflightChecks(this IServiceCollection services)
    {
        // Register MainPreflightManager as a singleton
        services.AddSingleton<IMainPreflightManager, MainPreflightManager>();

        // Register IPreflightSubManager as a scoped service
        services.AddScoped<IPreflightSubManager>();

        // Add MessagePipe with custom options if necessary
        services.AddMessagePipe(_ =>
        {
            // Customize MessagePipe options if necessary
        });

        // Add AppLogger for logging
        services.AddAppLogger();

        return services;
    }
}