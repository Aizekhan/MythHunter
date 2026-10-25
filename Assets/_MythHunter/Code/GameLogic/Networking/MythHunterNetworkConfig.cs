using RPGFramework.Networking;

namespace MythHunter.Networking
{
    /// <summary>
    /// MythHunter-specific network configuration.
    /// </summary>
    public static class MythHunterNetworkConfig
    {
        /// <summary>
        /// Default port for MythHunter multiplayer.
        /// </summary>
        public const int DefaultPort = 7777;

        /// <summary>
        /// Maximum players per game.
        /// </summary>
        public const int MaxPlayers = 16;

        /// <summary>
        /// Network tick rate (Hz).
        /// </summary>
        public const int TickRate = 30;

        /// <summary>
        /// Create MythHunter network configuration.
        /// </summary>
        public static NetworkConfig CreateConfig()
        {
            return new NetworkConfig
            {
                MaxConnections = MaxPlayers,
                ConnectionTimeout = 30f,
                ReplicationRate = TickRate,
                UseDeltaCompression = true,
                MaxBandwidth = 0, // Unlimited for now
                EnableLagCompensation = true,
                EnablePrediction = true,
                InterpolationDelay = 0.1f,
                PingInterval = 2f
            };
        }

        /// <summary>
        /// Create configuration for local testing.
        /// </summary>
        public static NetworkConfig CreateLocalTestConfig()
        {
            var config = CreateConfig();
            config.ReplicationRate = 60; // Higher rate for local testing
            config.PingInterval = 0.5f;
            return config;
        }
    }
}
