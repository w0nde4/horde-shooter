using System.Collections.Generic;
using _Project.Scripts.Core.Interfaces;
using _Project.Scripts.Domain.Gameplay;
using _Project.Scripts.Domain.Gameplay.Configs;
using _Project.Scripts.Domain.Gameplay.Signals;
using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Services.Network
{
    public class NetworkPlayerSpawnService : IPlayerSpawnService
    {
        private readonly AddressableService _addressableService;
        private readonly FusionNetworkService _networkService;
        private readonly PlayerConfig _config;
        private readonly PlayerRegistry _registry;
        private readonly SignalBus _signalBus;
        private readonly DiContainer _diContainer;

        [Inject]
        public NetworkPlayerSpawnService(AddressableService addressableService,
            FusionNetworkService networkService, PlayerConfig playerConfig,
            PlayerRegistry playerRegistry, SignalBus signalBus, DiContainer diContainer)
        {
            _addressableService = addressableService;
            _networkService = networkService;
            _config = playerConfig;
            _registry = playerRegistry;
            _signalBus = signalBus;
            _diContainer = diContainer;
        }
        
        public async UniTask<GameObject> SpawnPlayerAsync(string ownerID, Vector3 position, Quaternion rotation, int? playerRefRaw = null)
        {
            if (!playerRefRaw.HasValue)
            {
                Debug.LogError("NetworkPlayerSpawnService: playerRefRaw is required for networked spawn");

                return null;
            }

            var runner = _networkService.Runner;
            if (runner == null)
            {
                Debug.LogError("NetworkPlayerSpawnService: runner is not available.");
                return null;
            }

            var prefab = await _addressableService.LoadPrefabAsync(_config.playerPrefabAddress);
            if (prefab == null) return null;

            var networkObject = prefab.GetComponent<NetworkObject>();
            if (networkObject == null)
            {
                Debug.LogError("NetworkPlayerSpawnService: player prefab has no NetworkObject component.");
                return null;
            }

            var inputAuthority = PlayerRef.FromEncoded(playerRefRaw.Value);
            var spawned = runner.Spawn(networkObject, position, rotation, inputAuthority);
            
            if (spawned == null) return null;
            
            _registry.RegisterPlayer(spawned.gameObject, ownerID);
            var stats = ResolvePlayerStats(spawned.gameObject);
            _signalBus.Fire(new PlayerSpawnedSignal(spawned.gameObject, stats, ownerID));
            return spawned.gameObject;
        }

        private PlayerStats ResolvePlayerStats(GameObject player)
        {
            var ctx = player.GetComponent<GameObjectContext>();
            return ctx?.Container.Resolve<PlayerStats>();
        }

        public void DespawnPlayer(string ownerID, bool destroyObject = true)
        {
            if(!_registry.TryGetPlayer(ownerID, out var player)) return;
            _registry.UnregisterPlayer(ownerID);
            if (!destroyObject || player == null) return;

            var networkObject = player.GetComponent<NetworkObject>();
            if(networkObject != null && networkObject.IsValid && _networkService.Runner != null)
                _networkService.Runner.Despawn(networkObject);
            else
                Object.Destroy(player);
        }

        public void DespawnAll(bool destroyObjects = true)
        {
            var players = new List<GameObject>(_registry.Players);
            _registry.Clear();
            if(!destroyObjects) return;

            var runner = _networkService.Runner;
            foreach (var player in players)
            {
                if (player == null) continue;
                var networkObject = player.GetComponent<NetworkObject>();
                if(networkObject != null && networkObject.IsValid && runner != null)
                    runner.Despawn(networkObject);
                else
                    Object.Destroy(player);
            }
        }
    }
}