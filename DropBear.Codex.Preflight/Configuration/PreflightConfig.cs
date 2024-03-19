namespace DropBear.Codex.Preflight.Configuration;

/// <summary>
///     Configuration settings for preflight tasks, including retry and timeout parameters.
/// </summary>
public class PreflightConfig
{
    public PreflightConfig()
    {
        MaxRetryAttempts = 3;
        RetryDelay = TimeSpan.FromSeconds(1);
        TaskTimeout = TimeSpan.FromSeconds(30);
    }

    /// <summary>
    ///     Gets the maximum number of retry attempts for a task. Must be non-negative.
    /// </summary>
    public int MaxRetryAttempts { get; private set; }

    /// <summary>
    ///     Gets the delay between retry attempts. Must be non-negative.
    /// </summary>
    public TimeSpan RetryDelay { get; private set; }

    /// <summary>
    ///     Gets or sets the maximum timeout for a task to complete. Must be positive.
    /// </summary>
    public TimeSpan TaskTimeout { get; private set; }

    /// <summary>
    ///     Updates the configuration settings based on another PreflightConfig instance.
    /// </summary>
    /// <param name="otherConfig">The PreflightConfig instance to copy settings from.</param>
    public void CopyFrom(PreflightConfig otherConfig)
    {
        if (otherConfig == null)
            throw new ArgumentNullException(nameof(otherConfig), "Other configuration cannot be null.");

        MaxRetryAttempts = otherConfig.MaxRetryAttempts;
        RetryDelay = otherConfig.RetryDelay;
        TaskTimeout = otherConfig.TaskTimeout;
    }

    /// <summary>
    ///     Allows updating retry attempts and retry delay. Ensures values are within valid ranges.
    /// </summary>
    /// <param name="maxRetryAttempts">The maximum number of retry attempts. Must be non-negative.</param>
    /// <param name="retryDelay">The delay between retry attempts. Must be non-negative.</param>
    public void UpdateRetrySettings(int maxRetryAttempts, TimeSpan retryDelay)
    {
        if (maxRetryAttempts < 0)
            throw new ArgumentOutOfRangeException(nameof(maxRetryAttempts), "MaxRetryAttempts must be non-negative.");

        if (retryDelay < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(retryDelay), "RetryDelay must be non-negative.");

        MaxRetryAttempts = maxRetryAttempts;
        RetryDelay = retryDelay;
    }

    /// <summary>
    ///     Sets the task timeout, ensuring the value is positive.
    /// </summary>
    /// <param name="taskTimeout">The maximum timeout for a task to complete.</param>
    public void SetTaskTimeout(TimeSpan taskTimeout)
    {
        if (taskTimeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(taskTimeout), "TaskTimeout must be positive.");

        TaskTimeout = taskTimeout;
    }
}