using DropBear.Codex.Preflight.Configuration;
using DropBear.Codex.Preflight.Enums;

namespace DropBear.Codex.Preflight.Interfaces;

/// <summary>
///     Defines the interface for the main preflight manager responsible for coordinating multiple preflight sub-managers.
/// </summary>
public interface IMainPreflightManager
{
    /// <summary>
    ///     Registers a sub-manager to be coordinated by this main manager.
    /// </summary>
    /// <param name="subManager">The sub-manager to register.</param>
    // ReSharper disable once UnusedMember.Global
    void RegisterSubManager(IPreflightSubManager subManager);

    /// <summary>
    ///     Retrieves the current states of all registered sub-managers.
    /// </summary>
    /// <returns>A read-only dictionary with sub-manager IDs as keys and their states as values.</returns>
    // ReSharper disable once UnusedMember.Global
    IReadOnlyDictionary<string, TaskState> GetSubManagerStates();

    /// <summary>
    ///     Updates the default configuration settings for all sub-managers and their associated tasks.
    /// </summary>
    /// <param name="updateAction">An action that modifies the configuration settings.</param>
    // ReSharper disable once UnusedMember.Global
    void UpdateDefaultConfig(Action<PreflightConfig> updateAction);
}