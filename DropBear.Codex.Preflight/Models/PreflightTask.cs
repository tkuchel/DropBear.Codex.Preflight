using DropBear.Codex.Preflight.Configuration;
using DropBear.Codex.Preflight.Enums;
using DropBear.Codex.Preflight.Interfaces;
using DropBear.Codex.Preflight.Messages;
using MessagePipe;

namespace DropBear.Codex.Preflight.Models;

public abstract class BasePreflightTask : IPreflightTask
{
    private readonly IPublisher<TaskErrorMessage> _errorPublisher;
    private readonly IPublisher<TaskProgressMessage> _progressPublisher;
    private readonly IPublisher<TaskStateMessage> _statePublisher;

    public BasePreflightTask(
        string id,
        IPublisher<TaskStateMessage> statePublisher,
        IPublisher<TaskProgressMessage> progressPublisher,
        IPublisher<TaskErrorMessage> errorPublisher,
        bool mustSucceed = true)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        _statePublisher = statePublisher ?? throw new ArgumentNullException(nameof(statePublisher));
        _progressPublisher = progressPublisher ?? throw new ArgumentNullException(nameof(progressPublisher));
        _errorPublisher = errorPublisher ?? throw new ArgumentNullException(nameof(errorPublisher));
        MustSucceed = mustSucceed;
        MaxRetryAttempts = 3; 
        RetryDelay = TimeSpan.FromSeconds(1); 
    }

    public string Id { get; }
    public TaskState State { get; protected set; }
    public bool MustSucceed { get; }
    public int MaxRetryAttempts { get; protected set; }
    public TimeSpan RetryDelay { get; protected set; }

    // Abstract method that derived classes will implement with their specific task logic.
    protected abstract Task<bool> ExecuteTaskAsync(CancellationToken cancellationToken);

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

    public void ApplyConfig(PreflightConfig config)
    {
        ArgumentNullException.ThrowIfNull(config, nameof(config));
        MaxRetryAttempts = config.MaxRetryAttempts;
        RetryDelay = config.RetryDelay;
    }
}