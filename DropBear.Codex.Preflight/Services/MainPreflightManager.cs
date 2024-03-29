using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using Cysharp.Text;
using DropBear.Codex.AppLogger.Interfaces;
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
    private readonly IAppLogger<MainPreflightManager> _logger;
    private readonly List<IPreflightSubManager> _subManagers = [];

    private readonly ConcurrentDictionary<string, TaskState> _subManagerStates = new();

    private readonly IDisposable _subscription;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MainPreflightManager" /> class.
    /// </summary>
    /// <param name="subscriber">The subscriber for receiving sub-manager state change notifications.</param>
    /// <param name="logger">An instance of AppLogger for use.</param>
    public MainPreflightManager(ISubscriber<SubManagerStateChange> subscriber, IAppLogger<MainPreflightManager> logger)
    {
        ArgumentNullException.ThrowIfNull(subscriber, nameof(subscriber));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _logger = logger;
        _logger.LogDebug("Initializing MainPreflightManager.");

        _subscription = subscriber.Subscribe(OnSubManagerStateChange);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _logger.LogDebug("Disposing MainPreflightManager.");
        _subscription.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public void RegisterSubManager(IPreflightSubManager subManager)
    {
        ArgumentNullException.ThrowIfNull(subManager, nameof(subManager));

        _logger.LogDebug(ZString.Format("Registering sub-manager with ID {0}.", subManager.Id));

        subManager.Configure(_defaultConfig);
        _subManagers.Add(subManager);
        _subManagerStates[subManager.Id] = TaskState.Pending; // Initial state of Pending for new sub-managers

        _logger.LogInformation(ZString.Format("Sub-manager {0} registered successfully with initial state set to Pending.", subManager.Id));
    }

    /// <inheritdoc />
    public void UpdateDefaultConfig(Action<PreflightConfig> updateAction)
    {
        ArgumentNullException.ThrowIfNull(updateAction, nameof(updateAction));

        _logger.LogDebug("Updating default configuration for sub-managers.");

        var tempConfig = new PreflightConfig();
        updateAction(tempConfig);
        ValidateConfig(tempConfig);

        _defaultConfig.CopyFrom(tempConfig);
        foreach (var subManager in _subManagers)
        {
            subManager.Configure(_defaultConfig);
            _logger.LogDebug(ZString.Format("Sub-manager {0} configuration updated.", subManager.Id));
        }
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, TaskState> GetSubManagerStates()
    {
        _logger.LogDebug("Fetching current states of all registered sub-managers.");
        return new ReadOnlyDictionary<string, TaskState>(_subManagerStates);
    }

    // React to sub-manager state changes
    private void OnSubManagerStateChange(SubManagerStateChange change)
    {
        _logger.LogInformation(ZString.Format("Received state change for sub-manager {0}: {1}.", change.SubManagerId, change.State));

        _subManagerStates.AddOrUpdate(change.SubManagerId, change.State, (_, _) => change.State);

        ReactToStateChange(change.SubManagerId, change.State);
    }

    // Placeholder method for reacting to state changes
    private void ReactToStateChange(string subManagerId, TaskState newState)
    {
        _logger.LogInformation(ZString.Format("Reacting to state change for sub-manager {0}: {1}.", subManagerId, newState));
        // Any specific reactions to state changes can be implemented here
    }


    // Validate the configuration
    private static void ValidateConfig(PreflightConfig config)
    {
        if (config.MaxRetryAttempts < 1)
        {
            throw new ArgumentException("MaxRetryAttempts must be at least 1", nameof(config));
        }
    }
}