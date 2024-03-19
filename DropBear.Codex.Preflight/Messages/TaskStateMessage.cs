using DropBear.Codex.Preflight.Enums;

namespace DropBear.Codex.Preflight.Messages;

/// <summary>
///     Represents a message indicating a task's state change.
/// </summary>
public class TaskStateMessage
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TaskStateMessage" /> class.
    /// </summary>
    /// <param name="taskId">The unique identifier of the task.</param>
    /// <param name="state">The new state of the task.</param>
    public TaskStateMessage(string taskId, TaskState state)
    {
        TaskId = taskId ?? throw new ArgumentNullException(nameof(taskId));
        State = state;
    }

    /// <summary>
    ///     Gets the unique identifier for the task.
    /// </summary>
    public string TaskId { get; }

    /// <summary>
    ///     Gets the new state of the task.
    /// </summary>
    public TaskState State { get; }
}