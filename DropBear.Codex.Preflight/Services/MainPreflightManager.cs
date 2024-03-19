using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using DropBear.Codex.Preflight.Configuration;
using DropBear.Codex.Preflight.Enums;
using DropBear.Codex.Preflight.Interfaces;
using DropBear.Codex.Preflight.Messages;
using MessagePipe;

namespace DropBear.Codex.Preflight.Services;

/// <summary>
///     Manages the coordination and configuration of multiple preflight sub-managers, monitoring their states and
///     configurations.
/// </summary>
public class MainPreflightManager : IMainPreflightManager, IDisposable
{
    private readonly PreflightConfig _defaultConfig = new();
    private readonly List<IPreflightSubManager> _subManagers = new();
    private readonly ConcurrentDictionary<string, TaskState> _subManagerStates = new();
    private readonly IDisposable _subscription;

    public MainPreflightManager(ISubscriber<SubManagerStateChange> subscriber)
    {
        _subscription = subscriber.Subscribe(OnSubManagerStateChange);
    }

    public void Dispose()
    {
        _subscription.Dispose();
    }

    public void RegisterSubManager(IPreflightSubManager subManager)
    {
        ArgumentNullException.ThrowIfNull(subManager);
        subManager.Configure(_defaultConfig);
        _subManagers.Add(subManager);
        // Assuming an initial state of Pending for new sub-managers
        _subManagerStates[subManager.Id] = TaskState.Pending;
    }

    public void UpdateDefaultConfig(Action<PreflightConfig> updateAction)
    {
        ArgumentNullException.ThrowIfNull(updateAction);

        // Validate configuration before applying
        var tempConfig = new PreflightConfig();
        updateAction(tempConfig);
        ValidateConfig(tempConfig); // Implement this method based on your configuration validation logic

        _defaultConfig.CopyFrom(tempConfig); // Assuming a method to copy settings from one config to another
        foreach (var subManager in _subManagers) subManager.Configure(_defaultConfig);
    }

    public IReadOnlyDictionary<string, TaskState> GetSubManagerStates()
    {
        return new ReadOnlyDictionary<string, TaskState>(_subManagerStates);
    }

    private void OnSubManagerStateChange(SubManagerStateChange change)
    {
        _subManagerStates.AddOrUpdate(change.SubManagerId, change.State, (_, _) => change.State);

        // React to significant state changes
        ReactToStateChange(change.SubManagerId, change.State);
    }

    private static void ReactToStateChange(string subManagerId, TaskState newState)
    {
        // Placeholder for logic to react to state changes, e.g., logging or triggering further actions
        Console.WriteLine($"Sub-manager {subManagerId} changed state to {newState}.");
    }

    private static void ValidateConfig(PreflightConfig config)
    {
        // Implement validation logic here
        // Throw an exception if the configuration is not valid
        if (config.MaxRetryAttempts < 1)
            throw new ArgumentException("MaxRetryAttempts must be at least 1", nameof(config.MaxRetryAttempts));
    }
}