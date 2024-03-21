using DropBear.Codex.Preflight.Configuration;
using DropBear.Codex.Preflight.Enums;

namespace DropBear.Codex.Preflight.Interfaces;

/// <summary>
///     Defines the interface for a preflight task, which represents a unit of work to be executed with retry logic.
/// </summary>
public interface IPreflightTask
{
    /// <summary>
    ///     Gets the unique identifier for the task.
    /// </summary>
    string Id { get; }

    /// <summary>
    ///     Indicates whether the task must succeed for the preflight check to pass.
    /// </summary>
    bool MustSucceed { get; }

    /// <summary>
    ///     Gets the maximum number of retry attempts for executing the task.
    /// </summary>
    // ReSharper disable once UnusedMemberInSuper.Global
    int MaxRetryAttempts { get; }

    /// <summary>
    ///     Gets the delay between retry attempts.
    /// </summary>
    // ReSharper disable once UnusedMemberInSuper.Global
    TimeSpan RetryDelay { get; }

    /// <summary>
    ///     Gets the current state of the task.
    /// </summary>
    // ReSharper disable once UnusedMemberInSuper.Global
    TaskState State { get; }

    /// <summary>
    ///     Executes the task with retry logic, optionally publishing state, progress, and error messages.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, indicating the success or failure of the task.</returns>
    Task<bool> ExecuteWithRetryAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Applies configuration settings to the task.
    /// </summary>
    /// <param name="config">The configuration to apply.</param>
    void ApplyConfig(PreflightConfig config);
}