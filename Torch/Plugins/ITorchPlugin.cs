using System;

namespace Torch.Plugins
{
    public interface ITorchPlugin : IDisposable
    {
        /// <summary>
        ///     A unique ID for the plugin.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        ///     The version of the plugin.
        /// </summary>
        string Version { get; }

        /// <summary>
        ///     The name of the plugin.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Plugin's enabled state. Mainly for UI niceness
        /// </summary>
        PluginState State { get; }

        /// <summary>
        ///     This is called before the game loop is started.
        /// </summary>
        /// <param name="torchBase">Torch instance</param>
        void Init(ITorchBase torchBase);

        /// <summary>
        ///     This is called on the game thread after each tick.
        /// </summary>
        void Update();
    }

    public enum PluginState
    {
        NotInitialized,
        DisabledError,
        DisabledUser,
        UpdateRequired,
        UninstallRequested,
        NotInstalled,
        MissingDependency,
        Enabled
    }
}