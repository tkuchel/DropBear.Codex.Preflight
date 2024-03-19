namespace DropBear.Codex.Preflight.Interfaces;

public interface IAppLogger<T>
{
    /// <summary>
    /// Logs a message at the Debug level.
    /// </summary>
    /// <param name="message">The message to log.</param>
    void LogDebug(string message);

    /// <summary>
    /// Logs a message at the Information level.
    /// </summary>
    /// <param name="message">The message to log.</param>
    void LogInformation(string message);

    /// <summary>
    /// Logs a message at the Warning level.
    /// </summary>
    /// <param name="message">The message to log.</param>
    void LogWarning(string message);

    /// <summary>
    /// Logs a message at the Error level.
    /// </summary>
    /// <param name="message">The message to log.</param>
    void LogError(string message);

    /// <summary>
    /// Logs an exception and an optional message at the Error level.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">The optional message to log alongside the exception.</param>
    void LogError(Exception exception, string message = "");

    /// <summary>
    /// Logs a message at the Critical level.
    /// </summary>
    /// <param name="message">The message to log.</param>
    void LogCritical(string message);

    /// <summary>
    /// Logs an exception and an optional message at the Critical level.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">The optional message to log alongside the exception.</param>
    void LogCritical(Exception exception, string message = "");
}