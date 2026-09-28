using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Domain.Gameplay
{
    public class ZenjectNetworkObjectProvider : NetworkObjectProviderDefault, INetworkRunnerCallbacks
    {
        private readonly List<GameObject> _pendingObjects = new();
        private SceneContext _cachedSceneContext;

        protected override NetworkObject InstantiatePrefab(NetworkRunner runner, NetworkObject prefab)
        {
            var wasActive = prefab.gameObject.activeSelf;
            if(wasActive) prefab.gameObject.SetActive(false);
            
            var go = Instantiate(prefab.gameObject);
            if(wasActive) prefab.gameObject.SetActive(true);
            
            var no = go.GetComponent<NetworkObject>();
            
            if (_cachedSceneContext == null)
            {
                _cachedSceneContext = FindFirstObjectByType<SceneContext>();
            }

            if (_cachedSceneContext != null && _cachedSceneContext.Container != null)
            {
                _cachedSceneContext.Container.InjectGameObject(go);
                go.SetActive(true);
                Debug.Log($"[NETWORK] {go.name} injected successfully before Awake through SceneContext.");
            }
            else
            {
                lock (_pendingObjects)
                {
                    _pendingObjects.Add(go);
                }

                Debug.LogWarning($"[NETWORK] SceneContext not found. {go.name} added to scene load waiting queue.");
            }

            return no;
        }


        public void OnSceneLoadDone(NetworkRunner runner)
        {
            Debug.Log("[NETWORK] Fusion event: Scene loaded successfully. Start injecting.");
            _cachedSceneContext = FindFirstObjectByType<SceneContext>();
            InjectAllPendingObjects();
        }

        private void InjectAllPendingObjects()
        {
            if (_cachedSceneContext == null || _cachedSceneContext.Container == null)
            {
                Debug.LogError("[NETWORK] Fatal error: Scene loaded, but SceneContext is null! Pended objects is not injected.");
                return;
            }

            lock (_pendingObjects)
            {
                foreach (var go in _pendingObjects)
                {
                    if (go != null)
                    {
                        _cachedSceneContext.Container.InjectGameObject(go);
                        go.SetActive(true);
                        Debug.Log($"[EVENT INJECT] Pended object {go.name} injected successfully through OnSceneLoadDone.");
                    }
                }

                _pendingObjects.Clear();
            }
        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {
            _cachedSceneContext = null; 
        }

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
        }

        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
        }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
        }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request,
            byte[] token)
        {
        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key,
            ReadOnlySpan<byte> data)
        {
        }

        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
        {
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
        }

        public void OnConnectedToServer(NetworkRunner runner)
        {
        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
        }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {
        }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
        }
    }
}