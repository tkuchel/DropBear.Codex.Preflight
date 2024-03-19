namespace DropBear.Codex.Preflight.Enums;

/// <summary>
/// Represents the possible states of a task within the preflight check process.
/// </summary>
public enum TaskState
{
    /// <summary>
    /// The task is pending execution.
    /// </summary>
    Pending,

    /// <summary>
    /// The task is currently running.
    /// </summary>
    Running,

    /// <summary>
    /// The task has completed successfully.
    /// </summary>
    Completed,

    /// <summary>
    /// The task has failed to complete successfully.
    /// </summary>
    Failed
}
