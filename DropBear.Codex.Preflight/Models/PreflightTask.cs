using DropBear.Codex.Preflight.Configuration;
using DropBear.Codex.Preflight.Enums;
using DropBear.Codex.Preflight.Interfaces;
using DropBear.Codex.Preflight.Messages;
using MessagePipe;

namespace DropBear.Codex.Preflight.Models;

/// <summary>
///     Represents a task to be executed as part of the preflight check process, supporting retry logic.
/// </summary>
public class PreflightTask : IPreflightTask
{
    private readonly IPublisher<TaskErrorMessage> _errorPublisher;
    private readonly IPublisher<TaskProgressMessage> _progressPublisher;
    private readonly IPublisher<TaskStateMessage> _statePublisher;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PreflightTask" /> class.
    /// </summary>
    /// <param name="id">The unique identifier for the task.</param>
    /// <param name="taskFunc">The task to be executed, encapsulated as a Func returning a Task.</param>
    /// <param name="statePublisher">The publisher for task state messages.</param>
    /// <param name="progressPublisher">The publisher for task progress messages.</param>
    /// <param name="errorPublisher">The publisher for task error messages.</param>
    /// <param name="mustSucceed">Indicates whether the task must succeed for the preflight check to pass.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the required parameters are null.</exception>
    public PreflightTask(
        string id,
        Func<Task<bool>> taskFunc,
        IPublisher<TaskStateMessage> statePublisher,
        IPublisher<TaskProgressMessage> progressPublisher,
        IPublisher<TaskErrorMessage> errorPublisher,
        bool mustSucceed = true)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        TaskFunc = taskFunc ?? throw new ArgumentNullException(nameof(taskFunc));
        _statePublisher = statePublisher ?? throw new ArgumentNullException(nameof(statePublisher));
        _progressPublisher = progressPublisher ?? throw new ArgumentNullException(nameof(progressPublisher));
        _errorPublisher = errorPublisher ?? throw new ArgumentNullException(nameof(errorPublisher));
        MustSucceed = mustSucceed;
        MaxRetryAttempts = 3; // Default value, can be overridden by ApplyConfig
        RetryDelay = TimeSpan.FromSeconds(1); // Default value, can be overridden by ApplyConfig
    }

    private Func<Task<bool>> TaskFunc { get; }
    public TaskState State { get; private set; }

    public string Id { get; }
    public bool MustSucceed { get; }
    public int MaxRetryAttempts { get; private set; }
    public TimeSpan RetryDelay { get; private set; }

    /// <summary>
    ///     Executes the task with retry logic, publishing state, progress, and error messages as appropriate.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, indicating the success or failure of the task.</returns>
    public async Task<bool> ExecuteWithRetryAsync(CancellationToken cancellationToken)
    {
        _statePublisher.Publish(new TaskStateMessage(Id, TaskState.Running));
        for (var attempt = 0; attempt < MaxRetryAttempts; attempt++)
            try
            {
                var result = await TaskFunc().ConfigureAwait(false);
                if (!result) continue;

                _progressPublisher.Publish(new TaskProgressMessage(Id, 1.0));
                _statePublisher.Publish(new TaskStateMessage(Id, TaskState.Completed));
                State = TaskState.Completed;
                return true;
            }
            catch (Exception ex)
            {
                if (attempt == MaxRetryAttempts - 1) // On last attempt
                {
                    _errorPublisher.Publish(new TaskErrorMessage(Id, ex));
                    _statePublisher.Publish(new TaskStateMessage(Id, TaskState.Failed));
                    State = TaskState.Failed;
                }

                // Check for cancellation requested to break out of the loop early
                if (cancellationToken.IsCancellationRequested)
                {
                    _statePublisher.Publish(new TaskStateMessage(Id, TaskState.Failed));
                    State = TaskState.Failed;
                    throw new OperationCanceledException(cancellationToken);
                }

                await Task.Delay(RetryDelay, cancellationToken).ConfigureAwait(false);
            }

        return false;
    }

    /// <summary>
    ///     Applies configuration settings to the task.
    /// </summary>
    /// <param name="config">The configuration to apply.</param>
    public void ApplyConfig(PreflightConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        MaxRetryAttempts = config.MaxRetryAttempts;
        RetryDelay = config.RetryDelay;
    }
}