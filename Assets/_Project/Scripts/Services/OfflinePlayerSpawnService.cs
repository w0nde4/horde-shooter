using System.Collections.Generic;
using _Project.Scripts.Core.Interfaces;
using _Project.Scripts.Domain.Gameplay;
using _Project.Scripts.Domain.Gameplay.Configs;
using _Project.Scripts.Domain.Gameplay.Signals;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Services
{
    public class OfflinePlayerSpawnService : IPlayerSpawnService
    {
        private readonly ISpawnService _spawnService; 
        private readonly PlayerConfig _config;
        private readonly PlayerRegistry _registry;
        private readonly SignalBus _signalBus;
        
        [Inject]
        public OfflinePlayerSpawnService(ISpawnService spawnService, PlayerConfig config, PlayerRegistry registry,
            SignalBus signalBus)
        {
            _spawnService = spawnService;
            _config = config;
            _registry = registry;
            _signalBus = signalBus;
        }
        
        public async UniTask<GameObject> SpawnPlayerAsync(string ownerID, Vector3 position, Quaternion rotation, int? playerRefRaw = null)
        {
            if (string.IsNullOrEmpty(_config.playerPrefabAddress))
            {
                Debug.LogError("OfflinePlayerSpawnService: playerPrefabAddress is not set");
                return null;
            }

            var player = await _spawnService.SpawnAsync(_config.playerPrefabAddress, position, rotation);
            if (player == null)
            {
                Debug.LogError("OfflinePlayerSpawnService: failed to spawn player.");
                return null;
            }
            
            _registry.RegisterPlayer(player, ownerID);

            var playerStats = ResolvePlayerStats(player);
            _signalBus.Fire(new PlayerSpawnedSignal(player, playerStats, ownerID));
            
            return player;
        }

        private PlayerStats ResolvePlayerStats(GameObject player)
        {
            var ctx = player.GetComponent<GameObjectContext>();
            if (ctx == null)
            {
                Debug.LogError("PlayerSpawnService: player prefab has no GameObjectContext — " +
                               "PlayerStats cannot be resolved per-instance.");
                return null;
            }

            return ctx.Container.Resolve<PlayerStats>();
        }

        public void DespawnPlayer(string ownerID, bool destroyObject = true)
        {
            if (string.IsNullOrEmpty(ownerID)) return;

            if (!_registry.TryGetPlayer(ownerID, out var player))
            {
                Debug.LogWarning($"PlayerSpawnService: no player registered with id {ownerID}.");
                return;
            }
            
            _registry.UnregisterPlayer(ownerID);
            
            if (destroyObject && player != null) Object.Destroy(player);
        }

        public void DespawnAll(bool destroyObject = true)
        {
            var players = new List<GameObject>(_registry.Players);
            _registry.Clear();
            
            if(!destroyObject) return;
            foreach (var player in players)
                if(player != null) Object.Destroy(player);
        }
    }
}