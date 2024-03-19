using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using DropBear.Codex.Preflight.Configuration;
using DropBear.Codex.Preflight.Enums;
using DropBear.Codex.Preflight.Interfaces;
using DropBear.Codex.Preflight.Messages;
using MessagePipe;

namespace DropBear.Codex.Preflight.Services;

/// <summary>
///     Manages the coordination and configuration of multiple preflight sub-managers.
/// </summary>
public class MainPreflightManager : IMainPreflightManager, IDisposable
{
    private readonly PreflightConfig _defaultConfig = new();
    private readonly List<IPreflightSubManager> _subManagers = new();
    private readonly ConcurrentDictionary<string, TaskState> _subManagerStates = new();
    private readonly IDisposable _subscription;

    /// <summary>
    ///     Initializes a new instance of the MainPreflightManager class, subscribing to state change messages.
    /// </summary>
    /// <param name="subscriber">The subscriber to SubManagerStateChange messages.</param>
    public MainPreflightManager(ISubscriber<SubManagerStateChange> subscriber)
    {
        _subscription = subscriber.Subscribe(OnSubManagerStateChange);
    }

    /// <summary>
    ///     Disposes of resources used by the MainPreflightManager.
    /// </summary>
    public void Dispose()
    {
        _subscription.Dispose();
    }

    /// <summary>
    ///     Registers a sub-manager to be coordinated by this main manager and configures it with the default configuration.
    /// </summary>
    /// <param name="subManager">The sub-manager to register.</param>
    public void RegisterSubManager(IPreflightSubManager subManager)
    {
        ArgumentNullException.ThrowIfNull(subManager, nameof(subManager));
        subManager.Configure(_defaultConfig);
        _subManagers.Add(subManager);
    }

    /// <summary>
    ///     Updates the default configuration settings for all sub-managers and their associated tasks.
    /// </summary>
    /// <param name="updateAction">An action to modify the configuration settings.</param>
    public void UpdateDefaultConfig(Action<PreflightConfig> updateAction)
    {
        ArgumentNullException.ThrowIfNull(updateAction, nameof(updateAction));
        updateAction(_defaultConfig);
        foreach (var subManager in _subManagers) subManager.Configure(_defaultConfig);
    }

    /// <summary>
    ///     Retrieves the current states of all registered sub-managers.
    /// </summary>
    /// <returns>A read-only dictionary with sub-manager IDs as keys and their states as values.</returns>
    public IReadOnlyDictionary<string, TaskState> GetSubManagerStates()
    {
        return new ReadOnlyDictionary<string, TaskState>(_subManagerStates);
    }

    /// <summary>
    ///     Handles the state change of a sub-manager by updating the internal state tracking in a thread-safe manner.
    /// </summary>
    /// <param name="change">The state change message.</param>
    private void OnSubManagerStateChange(SubManagerStateChange change)
    {
        _subManagerStates.AddOrUpdate(change.SubManagerId, change.State, (id, existingVal) => change.State);
    }
}