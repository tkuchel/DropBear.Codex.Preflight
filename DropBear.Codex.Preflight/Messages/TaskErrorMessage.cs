namespace DropBear.Codex.Preflight.Messages;

/// <summary>
///     Represents an error message from a task.
/// </summary>
public class TaskErrorMessage
{
    /// <param name="taskId">Task's unique identifier.</param>
    /// <param name="error">The error encountered.</param>
    public TaskErrorMessage(string taskId, Exception error)
    {
        TaskId = taskId ?? throw new ArgumentNullException(nameof(taskId));
        Error = error ?? throw new ArgumentNullException(nameof(error));
    }

    /// <summary>
    ///     Gets the unique identifier for the task.
    /// </summary>
    public string TaskId { get; }

    /// <summary>
    ///     Gets the error encountered by the task.
    /// </summary>
    public Exception Error { get; }
}