using System;
using System.Collections.Generic;
using _Project.Scripts.Core.Signals;
using _Project.Scripts.Domain.Gameplay;
using Cysharp.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace _Project.Scripts.Services.Network
{
    public class FusionNetworkService : INetworkRunnerCallbacks, IInitializable
    {
        private NetworkRunner _runner;
        private NetworkSceneManagerDefault _sceneManager;
        private bool _isRunning;
        private ZenjectNetworkObjectProvider _provider;
        
        [Inject] private SignalBus _signalBus;
        [Inject] private DiContainer _container;
        
        public bool IsRunning => _runner != null && _runner.IsRunning;
        public bool IsServer => _runner != null && _runner.IsServer;
        public NetworkRunner Runner => _runner;
        
        public void Initialize()
        {
            var go = new GameObject("NetworkRunner");
            _runner = go.AddComponent<NetworkRunner>();
            _runner.AddCallbacks(this);
            _sceneManager = go.AddComponent<NetworkSceneManagerDefault>();

            _provider = _container.InstantiateComponent<ZenjectNetworkObjectProvider>(go);
            
            UnityEngine.Object.DontDestroyOnLoad(go);
        }
        
        public async UniTask StartGameAsync(GameMode gameMode, string sessionName, bool setScene = true)
        {
            if (_runner == null)
            {
                Debug.LogError("NetworkRunner is null.");
                return;
            }
        
            if (_isRunning)
            {
                Debug.LogWarning("Game is already running.");
                return;
            }
            
            var args = new StartGameArgs
            {
                GameMode = gameMode,
                SessionName = sessionName,
                SceneManager = _sceneManager,
                PlayerCount = 2,
                ObjectProvider = _provider
            };
            
            if (setScene) args.Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
            
            Debug.Log($"Starting game as: {gameMode} in session {sessionName}");

            try
            {
                var result = await _runner.StartGame(args);
                if (result.Ok)
                {
                    _isRunning = true;
                    Debug.Log("NetworkRunner started successfully.");
                }
                else
                {
                    Debug.LogError($"Failed to start game: {result.ShutdownReason}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception while starting game: {e.Message}");
            }
        }

        public void Shutdown()
        {
            if (_runner != null && _runner.IsRunning)
            {
                _runner.Shutdown();
                _isRunning = false;
            }
        }

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log($"Player joined: {player} (IsServer: {runner.IsServer})");
            
            if(runner.IsServer)
            {
                _signalBus.Fire(new PlayerJoinRequestSignal(player.ToString(), player.RawEncoded));
            }
        }
        
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log($"Player left: {player}");
            
            _signalBus.Fire(new PlayerLeaveRequestSignal(player.ToString(), runner.IsServer));
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            _isRunning = false;
        }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ReadOnlySpan<byte> data) { }

        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }

        public void OnInput(NetworkRunner runner, NetworkInput input) { }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

        public void OnConnectedToServer(NetworkRunner runner)
        {
            Debug.Log("CLIENT: Connected to server!");
        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

        public void OnSceneLoadDone(NetworkRunner runner) { }

        public void OnSceneLoadStart(NetworkRunner runner) { }
    }
}