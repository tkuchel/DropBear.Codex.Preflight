using DropBear.Codex.AppLogger.Extensions;
using DropBear.Codex.Preflight.Interfaces;
using DropBear.Codex.Preflight.Services;
using MessagePipe;
using Microsoft.Extensions.DependencyInjection;

namespace DropBear.Codex.Preflight;

public static class PreflightCheckServiceExtensions
{
    public static IServiceCollection AddPreflightChecks(this IServiceCollection services)
    {
        services.AddSingleton<IMainPreflightManager, MainPreflightManager>();
        services.AddScoped<IPreflightSubManager>();

        services.AddMessagePipe(_ =>
        {
            // Customize MessagePipe options if necessary
        });

        services.AddAppLogger();

        return services;
    }
}