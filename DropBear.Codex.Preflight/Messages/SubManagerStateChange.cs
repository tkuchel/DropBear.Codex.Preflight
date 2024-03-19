using DropBear.Codex.Preflight.Enums;

namespace DropBear.Codex.Preflight.Messages;

/// <summary>
///     Represents a message indicating a change in the state of a sub-manager.
/// </summary>
public class SubManagerStateChange
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SubManagerStateChange" /> class.
    /// </summary>
    /// <param name="subManagerId">The unique identifier of the sub-manager.</param>
    /// <param name="state">The new state of the sub-manager.</param>
    public SubManagerStateChange(string subManagerId, TaskState state)
    {
        SubManagerId = subManagerId ?? throw new ArgumentNullException(nameof(subManagerId));
        State = state;
    }

    /// <summary>
    ///     Gets the unique identifier for the sub-manager.
    /// </summary>
    public string SubManagerId { get; }

    /// <summary>
    ///     Gets the new state of the sub-manager.
    /// </summary>
    public TaskState State { get; }
}