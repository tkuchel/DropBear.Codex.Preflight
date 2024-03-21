using System.Collections.Concurrent;
using Cysharp.Text;
using DropBear.Codex.AppLogger.Interfaces;
using DropBear.Codex.Preflight.Configuration;
using DropBear.Codex.Preflight.Enums;
using DropBear.Codex.Preflight.Interfaces;
using DropBear.Codex.Preflight.Messages;
using MessagePipe;

namespace DropBear.Codex.Preflight.Services;

/// <summary>
///     Manages a collection of preflight tasks, coordinating their execution based on configurations and handling their
///     state changes, progress updates, and errors.
/// </summary>
public class PreflightSubManager : IPreflightSubManager
{
    private readonly ISubscriber<TaskErrorMessage> _errorSubscriber;
    private readonly IAppLogger<PreflightSubManager> _logger;
    private readonly ISubscriber<TaskProgressMessage> _progressSubscriber;
    private readonly IPublisher<SubManagerStateChange> _publisher;
    private readonly ISubscriber<TaskStateMessage> _stateSubscriber;
    private readonly ConcurrentQueue<IPreflightTask> _tasks = new();
    private PreflightConfig? _config;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PreflightSubManager" /> class.
    /// </summary>
    /// <param name="publisher">The publisher for state change notifications.</param>
    /// <param name="stateSubscriber">The subscriber for task state change notifications.</param>
    /// <param name="progressSubscriber">The subscriber for task progress change notifications.</param>
    /// <param name="errorSubscriber">The subscriber for task error notifications.</param>
    /// <param name="logger">The logger instance.</param>
    public PreflightSubManager(
        IPublisher<SubManagerStateChange> publisher,
        ISubscriber<TaskStateMessage> stateSubscriber,
        ISubscriber<TaskProgressMessage> progressSubscriber,
        ISubscriber<TaskErrorMessage> errorSubscriber,
        IAppLogger<PreflightSubManager> logger)
    {
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        Id = Guid.NewGuid().ToString();
        _stateSubscriber = stateSubscriber ?? throw new ArgumentNullException(nameof(stateSubscriber));
        _progressSubscriber = progressSubscriber ?? throw new ArgumentNullException(nameof(progressSubscriber));
        _errorSubscriber = errorSubscriber ?? throw new ArgumentNullException(nameof(errorSubscriber));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogDebug(ZString.Format("PreflightSubManager {0} initialized.", Id));

        SubscribeToMessages();
        ChangeState(TaskState.Pending); // Initialize state to Pending upon creation
    }

    /// <inheritdoc />
    public string Id { get; }

    /// <inheritdoc />
    public void AddTask(IPreflightTask task)
    {
        ArgumentNullException.ThrowIfNull(task, nameof(task));
        if (_config != null)
        {
            task.ApplyConfig(_config);
            _logger.LogInformation(ZString.Format("Config applied to task {0} in sub-manager {1}.", task.Id, Id));
        }

        _tasks.Enqueue(task);
        _logger.LogInformation(ZString.Format("Task {0} added to sub-manager {1}.", task.Id, Id));
    }

    /// <inheritdoc />
    public void Configure(PreflightConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger.LogDebug($"Configuring PreflightSubManager {Id}.");

        foreach (var task in _tasks)
        {
            task.ApplyConfig(config);
            _logger.LogDebug(ZString.Format("Task {0} configured in sub-manager {1}.", task.Id, Id));
        }
    }

    /// <inheritdoc />
    public async Task<bool> ExecuteTasksAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"PreflightSubManager {Id} starting task execution.");
        ChangeState(TaskState.Running);
        var overallSuccess = true;

        while (_tasks.TryDequeue(out var task))
        {
            _logger.LogInformation(ZString.Format("Executing task {0} in sub-manager {1}.", task.Id, Id));
            var success = await task.ExecuteWithRetryAsync(cancellationToken);
            if (success || !task.MustSucceed) continue;
            overallSuccess = false;
            _logger.LogWarning(ZString.Format("Task {0} critical failure in sub-manager {1}.", task.Id, Id));
            break; // Optional: Stop execution on first critical failure
        }

        ChangeState(overallSuccess ? TaskState.Completed : TaskState.Failed);
        _logger.LogInformation($"PreflightSubManager {Id} completed task execution with status: {overallSuccess}.");
        return overallSuccess;
    }

    // Subscribe to task state, progress, and error messages
    private void SubscribeToMessages()
    {
        _logger.LogDebug($"Subscribing to messages in PreflightSubManager {Id}.");

        _stateSubscriber.Subscribe(message =>
        {
            _logger.LogInformation(ZString.Format("Task {0} changed state to {1} in sub-manager {2}.",
                message.TaskId, message.State, Id));
        });

        _progressSubscriber.Subscribe(message =>
        {
            _logger.LogInformation(ZString.Format("Task {0} progress updated to {1:P} in sub-manager {2}.",
                message.TaskId, message.Progress, Id));
        });

        _errorSubscriber.Subscribe(message =>
        {
            _logger.LogError(ZString.Format("Task {0} encountered an error in sub-manager {1}: {2}", message.TaskId,
                Id, message.Error.Message));
        });
    }

    // Publish sub-manager state change
    private void ChangeState(TaskState newState)
    {
        _logger.LogDebug(ZString.Format("Sub-manager {0} changing state to {1}.", Id, newState));
        _publisher.Publish(new SubManagerStateChange(Id, newState));
    }
}