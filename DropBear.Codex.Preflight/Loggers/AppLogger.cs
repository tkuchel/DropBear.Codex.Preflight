using DropBear.Codex.Preflight.Interfaces;
using Microsoft.Extensions.Logging;
using ZLogger;

public class AppLogger<T> : IAppLogger<T>
{
    private readonly ILogger<T> _logger;

    public AppLogger(ILogger<T> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    ///     Logs a debug message.
    /// </summary>
    public void LogDebug(string message)
    {
        LogWithLevel(LogLevel.Debug, message);
    }

    /// <summary>
    ///     Logs an informational message.
    /// </summary>
    public void LogInformation(string message)
    {
        LogWithLevel(LogLevel.Information, message);
    }

    /// <summary>
    ///     Logs a warning message.
    /// </summary>
    public void LogWarning(string message)
    {
        LogWithLevel(LogLevel.Warning, message);
    }

    /// <summary>
    ///     Logs an error message.
    /// </summary>
    public void LogError(string message)
    {
        LogWithLevel(LogLevel.Error, message);
    }

    /// <summary>
    ///     Logs an error message along with an exception.
    /// </summary>
    public void LogError(Exception exception, string message = "")
    {
        LogWithLevel(LogLevel.Error, message, exception);
    }

    /// <summary>
    ///     Logs a critical error message.
    /// </summary>
    public void LogCritical(string message)
    {
        LogWithLevel(LogLevel.Critical, message);
    }

    /// <summary>
    ///     Logs a critical error message along with an exception.
    /// </summary>
    public void LogCritical(Exception exception, string message = "")
    {
        LogWithLevel(LogLevel.Critical, message, exception);
    }

    private void LogWithLevel(LogLevel level, string message, Exception exception = null)
    {
        if (_logger is ILogger<ZLogger.ZLoggerLogger>)
            // Call the appropriate ZLog method based on the LogLevel
            switch (level)
            {
                case LogLevel.Trace:
                    _logger.ZLogTrace($"Processed: {message}");
                    break;
                case LogLevel.Debug:
                    _logger.ZLogDebug($"Processed: {message}");
                    break;
                case LogLevel.Information:
                    _logger.ZLogInformation($"Processed: {message}");
                    break;
                case LogLevel.Warning:
                    _logger.ZLogWarning($"Processed: {message}");
                    break;
                case LogLevel.Error:
                    _logger.ZLogError(exception, $"Processed: {message}");
                    break;
                case LogLevel.Critical:
                    _logger.ZLogCritical(exception, $"Processed: {message}");
                    break;
                case LogLevel.None:
                default:
                    // Optionally handle other cases, or log a default message
                    _logger.Log(level, exception, message);
                    break;
            }
        else
            _logger.Log(level, exception, message);
    }
}