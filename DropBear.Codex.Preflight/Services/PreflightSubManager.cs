using System.Collections.Concurrent;
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

    public PreflightSubManager(
        IPublisher<SubManagerStateChange> publisher,
        string id,
        ISubscriber<TaskStateMessage> stateSubscriber,
        ISubscriber<TaskProgressMessage> progressSubscriber,
        ISubscriber<TaskErrorMessage> errorSubscriber,
        IAppLogger<PreflightSubManager> logger)
    {
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        Id = id ?? throw new ArgumentNullException(nameof(id));
        _stateSubscriber = stateSubscriber ?? throw new ArgumentNullException(nameof(stateSubscriber));
        _progressSubscriber = progressSubscriber ?? throw new ArgumentNullException(nameof(progressSubscriber));
        _errorSubscriber = errorSubscriber ?? throw new ArgumentNullException(nameof(errorSubscriber));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        SubscribeToMessages();
        ChangeState(TaskState.Pending); // Initialize state to Pending upon creation
    }

    public string Id { get; }

    public void AddTask(IPreflightTask task)
    {
        ArgumentNullException.ThrowIfNull(task, nameof(task));
        if (_config != null) task.ApplyConfig(_config);
        _tasks.Enqueue(task);
        _logger.LogInformation($"Task {task.Id} added to sub-manager {Id}.");
    }

    public void Configure(PreflightConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));

        // Apply the configuration to all tasks in the sub-manager
        foreach (var task in _tasks) task.ApplyConfig(config);
    }

    public async Task<bool> ExecuteTasksAsync(CancellationToken cancellationToken)
    {
        ChangeState(TaskState.Running);
        var overallSuccess = true;

        while (_tasks.TryDequeue(out var task))
        {
            var success = await task.ExecuteWithRetryAsync(cancellationToken);
            if (success || !task.MustSucceed) continue;
            overallSuccess = false; // Mark overall failure if any must-succeed task fails
            break; // Optional: Stop execution on first critical failure
        }

        ChangeState(overallSuccess ? TaskState.Completed : TaskState.Failed);
        return overallSuccess;
    }

    private void SubscribeToMessages()
    {
        // Simplified example of logging or handling task messages.
        // This placeholder setup should be replaced with actual logging or message handling logic as needed.
        _stateSubscriber.Subscribe(message =>
        {
            _logger.LogInformation($"Task {message.TaskId} changed state to {message.State}.");

            // Example: Updating internal state or UI based on task state change
            switch (message.State)
            {
                case TaskState.Running:
                    // Code to handle running state
                    break;
                case TaskState.Completed:
                    // Code to handle task completion
                    break;
                case TaskState.Failed:
                    // Code to handle task failure
                    break;
                case TaskState.Pending:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        });

        _progressSubscriber.Subscribe(message =>
        {
            _logger.LogInformation($"Task {message.TaskId} progress updated to {message.Progress:P}.");

            // Example: Updating progress bar or equivalent UI element
            // UpdateProgressBar(message.TaskId, message.Progress);
        });

        _errorSubscriber.Subscribe(message =>
        {
            _logger.LogError($"Task {message.TaskId} encountered an error: {message.Error.Message}");

            // Example: Handling specific error types differently
            // if (message.Error is SpecificExceptionType)
            // {
            //     // Handle specific error, e.g., retry task or notify user
            // }
        });
    }

    /// <summary>
    ///     Changes the state of the sub-manager, publishing the change to interested parties.
    /// </summary>
    /// <param name="newState">The new state of the sub-manager.</param>
    private void ChangeState(TaskState newState)
    {
        _publisher.Publish(new SubManagerStateChange(Id, newState));
    }
}