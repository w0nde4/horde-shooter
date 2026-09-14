using _Project.Scripts.Core.Interfaces;
using _Project.Scripts.Domain.Gameplay.Data;
using _Project.Scripts.Domain.Gameplay.Interfaces;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace _Project.Scripts.Services
{
    public class EnemySpawnService : IEnemySpawnService
    {
        private readonly ISpawnService _spawnService;
        
        [Inject]
        public EnemySpawnService(ISpawnService spawnService)
        {
            _spawnService = spawnService;
        }
        
        public async UniTask<GameObject> SpawnEnemyAsync(EnemySpawnContext ctx)
        {
            // === 1. Check if there is a player in the scene ===
            if (!HasAnyPlayer(ctx.PlayersTransforms))
            {
                Debug.LogWarning("WaveSpawner: No player found, skip spawning.");
                return null;
            }
            
            // === 2. Get a random spawn position around the player ===
            var spawnPos = GetRandomSpawnPosition(ctx.Center, ctx.MinSpawnRadius, ctx.MaxSpawnRadius);
            if (spawnPos == Vector3.zero)
            {
                Debug.LogWarning("Failed to find spawn position");
                return null;
            }

            // === 3. Instantiate the enemy prefab through ISpawnService ===
            var enemyObj = await _spawnService.SpawnAsync(
                ctx.PrefabAddress, 
                spawnPos, 
                Quaternion.identity,
                onBeforeSpawn: ctx.OnSetupObject
                );
            if (enemyObj == null)
            {
                Debug.LogError($"Failed to instantiate enemy from address {ctx.PrefabAddress}");
                return null;
            }

            return enemyObj;
        }

        private bool HasAnyPlayer(Transform[] playersPositions)
        {
            if (playersPositions == null || playersPositions.Length == 0) return false;
            foreach (var p in playersPositions)
                if (p != null) return true;
            return false;    
        }
        
        private Vector3 GetRandomSpawnPosition(Vector3 center, float minSpawnRadius, float maxSpawnRadius)
        {
            for (var i = 0; i < 30; i++)
            {
                var direction = Random.insideUnitCircle.normalized;
                var distance = Random.Range(minSpawnRadius, maxSpawnRadius);
                var candidate = center + new Vector3(direction.x, 0, direction.y) * distance;

                if (NavMesh.SamplePosition(candidate, out var hit, 2f, NavMesh.AllAreas))
                    return hit.position;
            }
            return Vector3.zero;
        }
    }
}