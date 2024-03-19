using DropBear.Codex.Preflight.Configuration;

namespace DropBear.Codex.Preflight.Interfaces;

/// <summary>
///     Defines the interface for a preflight sub-manager.
/// </summary>
public interface IPreflightSubManager
{
    string Id { get; }
    
    /// <summary>
    ///     Adds a preflight task to the sub-manager.
    /// </summary>
    /// <param name="task">The task to add.</param>
    void AddTask(IPreflightTask task);

    /// <summary>
    ///     Executes all tasks managed by the sub-manager asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing the result of the execution.</returns>
    Task<bool> ExecuteTasksAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Configures the sub-manager with specific settings.
    /// </summary>
    /// <param name="config">The configuration settings to apply.</param>
    void Configure(PreflightConfig config);
}