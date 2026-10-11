using System;
using _Project.Scripts.Core.Signals;
using _Project.Scripts.Domain.Gameplay.Signals;
using _Project.Scripts.Services;
using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Player
{
    public class CoopPlayerLifecycleManager : NetworkBehaviour
    {
        [SerializeField] private float respawnDelay = 5f;

        [Inject] private PlayerRegistry _playerRegistry;
        [Inject] private SignalBus _signalBus;

        public override void Spawned()
        {
            _playerRegistry.OnPlayerRegistered += OnPlayerRegistered;
            _playerRegistry.OnPlayerUnregistered += OnPlayerUnregistered;
        }

        public override void Despawned(NetworkRunner runner, bool hasStateAuthority)
        {
            _playerRegistry.OnPlayerRegistered -= OnPlayerRegistered;
            _playerRegistry.OnPlayerUnregistered -= OnPlayerUnregistered;
        }

        private void OnPlayerRegistered(GameObject playerObj)
        {
            if (!HasStateAuthority) return;

            if (playerObj.TryGetComponent<PlayerHealth>(out var health))
            {
                health.Server_OnPlayerDied += HandlePlayerDeath;
            }
        }

        private void OnPlayerUnregistered(GameObject playerObj)
        {
            if (!HasStateAuthority) return;

            if (playerObj.TryGetComponent<PlayerHealth>(out var health))
            {
                health.Server_OnPlayerDied -= HandlePlayerDeath;
            }
        }

        private void HandlePlayerDeath(PlayerHealth deadPlayerHealth)
        {
            if (_playerRegistry.HasAnyLivingPlayers())
            {
                var player = deadPlayerHealth.Object.InputAuthority;
                Debug.Log($"[GameplayManager] Player {player} died. Starting respawn timer.");
                Rpc_NotifyRespawnStarted(player);
                StartRespawnTimerAsync(deadPlayerHealth).Forget();
            }
            else
            {
                Debug.Log("[GameplayManager] All player are dead. Game over!");
                Rpc_BroadcastGameOver();
            }
        }

        private async UniTaskVoid StartRespawnTimerAsync(PlayerHealth playerHealth)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(respawnDelay));

            if (playerHealth == null || playerHealth.Object == null) return;

            var player = playerHealth.Object.InputAuthority;
            
            if (_playerRegistry.HasAnyLivingPlayers())
            {
                playerHealth.Server_Respawn();
                Rpc_NotifyRespawnEnded(player);
                Debug.Log($"[GameplayManager] Player {player} respawned.");
            }
            else
            {
                Rpc_NotifyRespawnEnded(player);
                Rpc_BroadcastGameOver();
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void Rpc_BroadcastGameOver()
        {
            _signalBus.Fire<GameOverStartedSignal>();
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void Rpc_NotifyRespawnStarted(PlayerRef player)
        {
            _signalBus.Fire(new PlayerRespawnStartedSignal(player));
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void Rpc_NotifyRespawnEnded(PlayerRef player)
        {
            _signalBus.Fire(new PlayerRespawnEndedSignal(player));
        }
    }
}