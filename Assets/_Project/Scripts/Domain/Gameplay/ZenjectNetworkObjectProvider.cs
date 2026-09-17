using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Domain.Gameplay
{
    public class ZenjectNetworkObjectProvider : NetworkObjectProviderDefault, INetworkRunnerCallbacks
    {
        private DiContainer _container;
        private readonly List<GameObject> _pendingObjects = new();

        [Inject]
        public void Construct(DiContainer container)
        {
            _container = container;
        }

        protected override NetworkObject InstantiatePrefab(NetworkRunner runner, NetworkObject prefab)
        {
            var go = Instantiate(prefab.gameObject);
            var no = go.GetComponent<NetworkObject>();

            var sceneContext = FindFirstObjectByType<SceneContext>();

            if (sceneContext != null && sceneContext.Container != null)
            {
                sceneContext.Container.InjectGameObject(go);
            }
            else
            {
                lock (_pendingObjects)
                {
                    _pendingObjects.Add(go);
                }

                InjectWithTimeoutAsync(go).Forget();
            }

            return no;
        }


        public void OnSceneLoadDone(NetworkRunner runner)
        {
            Debug.Log("[NETWORK] Событие Fusion: Сцена успешно загружена. Начинаем инжект объектов.");
            InjectAllPendingObjects();
        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {
        }

        private void InjectAllPendingObjects()
        {
            var sceneContext = FindFirstObjectByType<SceneContext>();
            if (sceneContext == null || sceneContext.Container == null) return;

            lock (_pendingObjects)
            {
                foreach (var go in _pendingObjects)
                {
                    if (go != null)
                    {
                        sceneContext.Container.InjectGameObject(go);
                        Debug.Log($"[EVENT INJECT] {go.name} успешно заинжекчен через OnSceneLoadDone.");
                    }
                }

                _pendingObjects.Clear();
            }
        }

        private async UniTaskVoid InjectWithTimeoutAsync(GameObject target)
        {
            var maxAttempts = 20;
            while (maxAttempts > 0 && target != null)
            {
                var sceneContext = FindFirstObjectByType<SceneContext>();
                if (sceneContext != null && sceneContext.Container != null)
                {
                    lock (_pendingObjects)
                    {
                        if (_pendingObjects.Contains(target))
                        {
                            sceneContext.Container.InjectGameObject(target);
                            _pendingObjects.Remove(target);
                            Debug.Log($"[TIMEOUT INJECT] {target.name} заинжекчен через резервное ожидание.");
                        }
                    }

                    return;
                }

                maxAttempts--;
                await UniTask.Delay(100);
            }

            if (target != null)
            {
                lock (_pendingObjects)
                {
                    if (_pendingObjects.Contains(target))
                    {
                        _container.InjectGameObject(target);
                        _pendingObjects.Remove(target);
                        Debug.LogWarning(
                            $"[FALLBACK INJECT] Не удалось дождаться сцены для {target.name}. Инжект в ProjectContext.");
                    }
                }
            }
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