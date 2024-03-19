namespace DropBear.Codex.Preflight.Configuration;

/// <summary>
///     Configuration settings for preflight tasks, including retry and timeout parameters.
/// </summary>
public class PreflightConfig
{
    private readonly int _maxRetryAttempts;
    private readonly TimeSpan _retryDelay;
    private TimeSpan _taskTimeout;

    public PreflightConfig()
    {
        MaxRetryAttempts = 3;
        RetryDelay = TimeSpan.FromSeconds(1);
        TaskTimeout = TimeSpan.FromSeconds(30);
    }

    /// <summary>
    ///     Gets or sets the maximum number of retry attempts for a task. Must be non-negative.
    /// </summary>
    public int MaxRetryAttempts
    {
        get => _maxRetryAttempts;
        private init
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "MaxRetryAttempts must be non-negative");
            _maxRetryAttempts = value;
        }
    }

    /// <summary>
    ///     Gets or sets the delay between retry attempts. Must be non-negative.
    /// </summary>
    public TimeSpan RetryDelay
    {
        get => _retryDelay;
        private init
        {
            if (value < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(value), "RetryDelay must be non-negative");
            _retryDelay = value;
        }
    }

    /// <summary>
    ///     Gets or sets the maximum timeout for a task to complete. Must be positive.
    /// </summary>
    public TimeSpan TaskTimeout
    {
        get => _taskTimeout;
        set
        {
            if (value <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(value), "TaskTimeout must be positive");
            _taskTimeout = value;
        }
    }
}