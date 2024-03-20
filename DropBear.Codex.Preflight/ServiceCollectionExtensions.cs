using DropBear.Codex.AppLogger.Extensions;
using DropBear.Codex.Preflight.Interfaces;
using DropBear.Codex.Preflight.Models;
using DropBear.Codex.Preflight.Services;
using MessagePipe;
using Microsoft.Extensions.DependencyInjection;

namespace DropBear.Codex.Preflight;

public static class PreflightCheckServiceExtensions
{
    public static IServiceCollection AddPreflightChecks(this IServiceCollection services)
    {
        services.AddSingleton<MainPreflightManager>();
        services.AddSingleton<IPreflightSubManager, PreflightSubManager>();
        services.AddSingleton<IPreflightTask, PreflightTask>();

        services.AddMessagePipe(_ =>
        {
            // Customize MessagePipe options if necessary
        });

        services.AddAppLogger();

        return services;
    }
}