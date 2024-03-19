namespace DropBear.Codex.Preflight.Messages;

/// <summary>
///     Represents a progress update message from a task.
/// </summary>
public class TaskProgressMessage
{
    /// <param name="taskId">Task's unique identifier.</param>
    /// <param name="progress">The current progress of the task.</param>
    public TaskProgressMessage(string taskId, double progress)
    {
        TaskId = taskId ?? throw new ArgumentNullException(nameof(taskId));
        Progress = progress is >= 0 and <= 1
            ? progress
            : throw new ArgumentOutOfRangeException(nameof(progress), "Progress must be between 0 and 1.");
    }

    /// <summary>
    ///     Gets the unique identifier for the task.
    /// </summary>
    public string TaskId { get; }

    /// <summary>
    ///     Gets the current progress of the task.
    /// </summary>
    public double Progress { get; }
}