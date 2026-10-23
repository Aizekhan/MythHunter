using RPGFramework.Networking;
using System;

namespace MythHunter.Networking
{
    /// <summary>
    /// Manages networked gameplay for MythHunter.
    /// Coordinates between NetworkManager, ReplicationSystem, and game systems.
    /// </summary>
    public class NetworkedGameManager
    {
        private readonly INetworkManager _networkManager;
        private readonly IReplicationSystem _replicationSystem;
        private readonly INetworkTransport _transport;
        private readonly INetworkSerializer _serializer;
        private bool _isRunning;

        public NetworkRole Role => _networkManager.Role;
        public bool IsConnected => _networkManager.IsConnected;

        public NetworkedGameManager(
            INetworkTransport transport,
            INetworkSerializer serializer = null,
            NetworkConfig? config = null)
        {
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
            _serializer = serializer ?? new NetworkSerializer();

            var networkConfig = config ?? MythHunterNetworkConfig.CreateConfig();

            _networkManager = new NetworkManager(_transport, _serializer, networkConfig);
            _replicationSystem = new ReplicationSystem(networkConfig);

            RegisterMessageHandlers();
        }

        /// <summary>
        /// Start as server (host).
        /// </summary>
        public void StartServer(int port = MythHunterNetworkConfig.DefaultPort)
        {
            _networkManager.StartServer(port);
            _isRunning = true;
        }

        /// <summary>
        /// Start as client and connect to server.
        /// </summary>
        public void StartClient(string address, int port = MythHunterNetworkConfig.DefaultPort)
        {
            _networkManager.StartClient(address, port);
            _isRunning = true;
        }

        /// <summary>
        /// Stop network operations.
        /// </summary>
        public void Stop()
        {
            _networkManager.Stop();
            _isRunning = false;
        }

        /// <summary>
        /// Update network systems. Call this every frame.
        /// </summary>
        public void Update(float deltaTime)
        {
            if (!_isRunning)
                return;

            _networkManager.Update(deltaTime);

            // Server replicates entities to clients
            if (_networkManager.Role == NetworkRole.Server)
            {
                _replicationSystem.ReplicateToClients(_networkManager, deltaTime);
            }
        }

        /// <summary>
        /// Spawn a network entity.
        /// </summary>
        public void SpawnNetworkEntity(int networkId, int ownerId, string prefabId, byte[] initialState = null)
        {
            if (_networkManager.Role != NetworkRole.Server)
                throw new InvalidOperationException("Only server can spawn entities");

            var spawnMessage = new EntitySpawnMessage
            {
                NetworkId = networkId,
                OwnerId = ownerId,
                PrefabId = prefabId,
                InitialState = initialState
            };

            _networkManager.Broadcast(spawnMessage, reliable: true);
        }

        /// <summary>
        /// Despawn a network entity.
        /// </summary>
        public void DespawnNetworkEntity(int networkId)
        {
            if (_networkManager.Role != NetworkRole.Server)
                throw new InvalidOperationException("Only server can despawn entities");

            var despawnMessage = new EntityDespawnMessage
            {
                NetworkId = networkId
            };

            _networkManager.Broadcast(despawnMessage, reliable: true);
            _replicationSystem.UnregisterEntity(networkId);
        }

        /// <summary>
        /// Register a network entity for replication.
        /// </summary>
        public void RegisterEntity(INetworkEntity entity)
        {
            _replicationSystem.RegisterEntity(entity);
        }

        /// <summary>
        /// Unregister a network entity.
        /// </summary>
        public void UnregisterEntity(int networkId)
        {
            _replicationSystem.UnregisterEntity(networkId);
        }

        /// <summary>
        /// Get network statistics.
        /// </summary>
        public NetworkStats GetStats()
        {
            return _networkManager.GetStats();
        }

        private void RegisterMessageHandlers()
        {
            _networkManager.RegisterMessageHandler<EntitySpawnMessage>(OnEntitySpawn);
            _networkManager.RegisterMessageHandler<EntityDespawnMessage>(OnEntityDespawn);
            _networkManager.RegisterMessageHandler<EntityStateUpdateMessage>(OnEntityStateUpdate);
            _networkManager.RegisterMessageHandler<EntityDeltaUpdateMessage>(OnEntityDeltaUpdate);
        }

        private void OnEntitySpawn(int connectionId, EntitySpawnMessage message)
        {
            // Game-specific logic to instantiate entity from prefab
            // This is where you'd integrate with your entity spawning system
        }

        private void OnEntityDespawn(int connectionId, EntityDespawnMessage message)
        {
            _replicationSystem.UnregisterEntity(message.NetworkId);
            // Game-specific logic to destroy entity
        }

        private void OnEntityStateUpdate(int connectionId, EntityStateUpdateMessage message)
        {
            _replicationSystem.ApplyReplication(message.NetworkId, message.StateData, isDelta: false);
        }

        private void OnEntityDeltaUpdate(int connectionId, EntityDeltaUpdateMessage message)
        {
            _replicationSystem.ApplyReplication(message.NetworkId, message.DeltaData, isDelta: true);
        }
    }
}
