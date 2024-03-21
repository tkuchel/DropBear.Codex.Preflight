using DropBear.Codex.Preflight.Configuration;
using DropBear.Codex.Preflight.Enums;
using DropBear.Codex.Preflight.Interfaces;
using DropBear.Codex.Preflight.Messages;
using MessagePipe;

namespace DropBear.Codex.Preflight.Models;

/// <summary>
///     Base class for preflight tasks, providing common functionality and properties.
/// </summary>
// ReSharper disable once UnusedType.Global
public abstract class BasePreflightTask : IPreflightTask
{
    private readonly IPublisher<TaskErrorMessage> _errorPublisher;
    private readonly IPublisher<TaskProgressMessage> _progressPublisher;
    private readonly IPublisher<TaskStateMessage> _statePublisher;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BasePreflightTask" /> class.
    /// </summary>
    /// <param name="statePublisher">The publisher for task state change notifications.</param>
    /// <param name="progressPublisher">The publisher for task progress change notifications.</param>
    /// <param name="errorPublisher">The publisher for task error notifications.</param>
    /// <param name="mustSucceed">Specifies whether the task must succeed for the overall process to succeed.</param>
    protected BasePreflightTask(
        IPublisher<TaskStateMessage> statePublisher,
        IPublisher<TaskProgressMessage> progressPublisher,
        IPublisher<TaskErrorMessage> errorPublisher,
        bool mustSucceed = true)
    {
        Id = Guid.NewGuid().ToString();
        _statePublisher = statePublisher ?? throw new ArgumentNullException(nameof(statePublisher));
        _progressPublisher = progressPublisher ?? throw new ArgumentNullException(nameof(progressPublisher));
        _errorPublisher = errorPublisher ?? throw new ArgumentNullException(nameof(errorPublisher));
        MustSucceed = mustSucceed;
        MaxRetryAttempts = 3;
        RetryDelay = TimeSpan.FromSeconds(1);
    }

    /// <inheritdoc />
    public string Id { get; }

    /// <inheritdoc />
    public TaskState State { get; private set; }

    /// <inheritdoc />
    public bool MustSucceed { get; }

    /// <inheritdoc />
    public int MaxRetryAttempts { get; private set; }

    /// <inheritdoc />
    public TimeSpan RetryDelay { get; private set; }

    /// <inheritdoc />
    public async Task<bool> ExecuteWithRetryAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < MaxRetryAttempts; attempt++)
        {
            _statePublisher.Publish(new TaskStateMessage(Id, TaskState.Running));
            try
            {
                var result = await ExecuteTaskAsync(cancellationToken).ConfigureAwait(false);
                if (!result) continue;
                _progressPublisher.Publish(new TaskProgressMessage(Id, 1.0));
                _statePublisher.Publish(new TaskStateMessage(Id, TaskState.Completed));
                State = TaskState.Completed;
                return true;
            }
            catch (Exception ex)
            {
                if (attempt == MaxRetryAttempts - 1) // Log error only on the last attempt
                    _errorPublisher.Publish(new TaskErrorMessage(Id, ex));

                _statePublisher.Publish(new TaskStateMessage(Id, TaskState.Failed));
                State = TaskState.Failed;
                cancellationToken.ThrowIfCancellationRequested();
                if (attempt < MaxRetryAttempts - 1)
                    await Task.Delay(RetryDelay, cancellationToken).ConfigureAwait(false);
            }
        }

        return false;
    }

    /// <inheritdoc />
    public void ApplyConfig(PreflightConfig config)
    {
        ArgumentNullException.ThrowIfNull(config, nameof(config));
        MaxRetryAttempts = config.MaxRetryAttempts;
        RetryDelay = config.RetryDelay;
    }

    // Abstract method that derived classes will implement with their specific task logic.
    protected abstract Task<bool> ExecuteTaskAsync(CancellationToken cancellationToken);
}