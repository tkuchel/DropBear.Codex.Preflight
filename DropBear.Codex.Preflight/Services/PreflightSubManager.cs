using System.Collections.Concurrent;
using DropBear.Codex.Preflight.Configuration;
using DropBear.Codex.Preflight.Enums;
using DropBear.Codex.Preflight.Interfaces;
using DropBear.Codex.Preflight.Messages;
using MessagePipe;

namespace DropBear.Codex.Preflight.Services;

/// <summary>
///     Manages a collection of preflight tasks, coordinating their execution based on configurations and handling their
///     state changes.
/// </summary>
public class PreflightSubManager : IPreflightSubManager
{
    private readonly ISubscriber<TaskErrorMessage> _errorSubscriber;
    private readonly string _id;
    private readonly ISubscriber<TaskProgressMessage> _progressSubscriber;
    private readonly IPublisher<SubManagerStateChange> _publisher;
    private readonly ISubscriber<TaskStateMessage> _stateSubscriber;
    private readonly ConcurrentQueue<IPreflightTask> _tasks = new();
    private PreflightConfig _config;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PreflightSubManager" /> class.
    /// </summary>
    /// <param name="publisher">The publisher for sub-manager state changes.</param>
    /// <param name="id">The unique identifier of the sub-manager.</param>
    /// <param name="stateSubscriber">The subscriber for task state messages.</param>
    /// <param name="progressSubscriber">The subscriber for task progress messages.</param>
    /// <param name="errorSubscriber">The subscriber for task error messages.</param>
    public PreflightSubManager(
        IPublisher<SubManagerStateChange> publisher,
        string id,
        ISubscriber<TaskStateMessage> stateSubscriber,
        ISubscriber<TaskProgressMessage> progressSubscriber,
        ISubscriber<TaskErrorMessage> errorSubscriber)
    {
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        _id = id ?? throw new ArgumentNullException(nameof(id));
        _stateSubscriber = stateSubscriber ?? throw new ArgumentNullException(nameof(stateSubscriber));
        _progressSubscriber = progressSubscriber ?? throw new ArgumentNullException(nameof(progressSubscriber));
        _errorSubscriber = errorSubscriber ?? throw new ArgumentNullException(nameof(errorSubscriber));

        SubscribeToMessages();
    }

    /// <summary>
    ///     Adds a preflight task to the manager, configuring it with the current settings.
    /// </summary>
    /// <param name="task">The task to be added and managed.</param>
    public void AddTask(IPreflightTask task)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));
        task.ApplyConfig(_config);
        _tasks.Enqueue(task);
    }

    /// <summary>
    ///     Configures the sub-manager and its managed tasks with specified settings.
    /// </summary>
    /// <param name="config">The configuration settings to apply.</param>
    public void Configure(PreflightConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>
    ///     Executes all managed tasks asynchronously, considering their individual must-succeed flags and retry policies.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, with a boolean indicating the overall success.</returns>
    public async Task<bool> ExecuteTasksAsync(CancellationToken cancellationToken)
    {
        var overallSuccess = true;
        while (_tasks.TryDequeue(out var task))
        {
            var success = await task.ExecuteWithRetryAsync(cancellationToken);
            if (!success && task.MustSucceed)
                overallSuccess = false;
        }

        return overallSuccess;
    }

    private void SubscribeToMessages()
    {
        _stateSubscriber.Subscribe(message =>
        {
            // Assuming Logger is your application's logging mechanism
            //Logger.Info($"Task {message.TaskId} state changed to {message.State}.");
            // Additionally, you could react to specific state changes here
        });

        _progressSubscriber.Subscribe(message =>
        {
            //Logger.Info($"Task {message.TaskId} progress updated: {message.Progress:P}.");
            // Handle specific progress-related logic here, if needed
        });

        _errorSubscriber.Subscribe(message =>
        {
            //Logger.Error($"Task {message.TaskId} encountered an error: {message.Error.Message}.");
            // Error handling or recovery actions could be placed here
        });
    }


    /// <summary>
    ///     Changes the state of the sub-manager, publishing the change to interested parties.
    /// </summary>
    /// <param name="newState">The new state of the sub-manager.</param>
    public void ChangeState(TaskState newState)
    {
        _publisher.Publish(new SubManagerStateChange(_id, newState));
    }
}