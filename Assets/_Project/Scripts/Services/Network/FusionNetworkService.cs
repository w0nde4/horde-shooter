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
            CreateAndConfigureRunner();
        }
        
        public async UniTask StartGameInternal(GameMode gameMode, string sessionName, string gameplaySceneName, int playersCount)
        {
            if (_runner == null || _isRunning) return;
           
            var sceneIndex = SceneUtility.GetBuildIndexByScenePath($"Assets/_Project/Scenes/{gameplaySceneName}.unity");
            if (sceneIndex < 0)
            {
                Debug.LogError($"Scene: {gameplaySceneName} is not in Build Settings!");
                return;
            }
            
            var args = new StartGameArgs
            {
                GameMode = gameMode,
                SessionName = sessionName,
                SceneManager = _sceneManager,
                PlayerCount = playersCount,
                ObjectProvider = _provider,
                Scene = SceneRef.FromIndex(sceneIndex)
            };
            
            var result = await _runner.StartGame(args); 
            if (result.Ok) _isRunning = true;
            else Debug.LogError($"Failed: {result.ShutdownReason}");
        }

        public async UniTask StartSingleplayerGameAsync(string gameplaySceneName = "Main")
        {
            await StartGameInternal(GameMode.Single, "SingleplayerSession", gameplaySceneName, 1);
        }

        public async UniTask StartMultiplayerHostAsync(string sessionName, int maxPlayers = 4, string gameplaySceneName = "Main")
        {
            await StartGameInternal(GameMode.Host, sessionName, gameplaySceneName, maxPlayers);
        }

        public async UniTask JoinMultiplayerAsync(string sessionName, string gameplaySceneName = "Main")
        {
            await StartGameInternal(GameMode.Client, sessionName, gameplaySceneName, 2);
        }

        public void Shutdown()
        {
            if (_runner != null && _runner.IsRunning)
                _runner.Shutdown();
            
            _isRunning = false;
        }

        private void CreateAndConfigureRunner()
        {
            var go = new GameObject("NetworkRunner");
            _runner = go.AddComponent<NetworkRunner>();
            _sceneManager = go.AddComponent<NetworkSceneManagerDefault>();
            _provider = _container.InstantiateComponent<ZenjectNetworkObjectProvider>(go);
            
            _runner.AddCallbacks(this);
            _runner.AddCallbacks(_provider);
            
            UnityEngine.Object.DontDestroyOnLoad(go);
        }
        
        private void RecreateRunner()
        {
            if (_runner != null)
                UnityEngine.Object.Destroy(_runner.gameObject);

            var go = new GameObject("NetworkRunner");
            _runner = go.AddComponent<NetworkRunner>();
            _runner.AddCallbacks(this);
            _sceneManager = go.AddComponent<NetworkSceneManagerDefault>();
            _provider = _container.InstantiateComponent<ZenjectNetworkObjectProvider>(go);
            UnityEngine.Object.DontDestroyOnLoad(go);
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