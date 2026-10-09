using System;
using _Project.Scripts.Core.Interfaces;
using _Project.Scripts.Domain.Gameplay.Data;
using _Project.Scripts.Domain.Gameplay.Interfaces;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using Zenject;
using Random = UnityEngine.Random;

namespace _Project.Scripts.Services
{
    public class EnemySpawnService : IEnemySpawnService
    {
        private const float SampleMaxDistance = 20f;
        private const int RandomAttempts = 30;
        
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
            
            // === 2. Get a valid NavMesh position for spawning ===
            if (!TryGetSpawnPosition(ctx, out var spawnPos))
            {
                Debug.LogWarning("EnemySpawnService: Failed to find NavMesh position for spawn.");
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
        
        private bool TryGetSpawnPosition(EnemySpawnContext ctx, out Vector3 result)
        {
            // === 3 levels of Fault-Tolerance ===
            
            // --- Level 1: random ring ---
            for (var i = 0; i < RandomAttempts; i++)
            {
                var angle = Random.value * (float)Math.PI * 2f;
                var direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                var distance = Random.Range(ctx.MinSpawnRadius, ctx.MaxSpawnRadius);
                var candidate = ctx.Center + new Vector3(direction.x, 0f, direction.y) * distance;

                if (NavMesh.SamplePosition(candidate, out var hit, SampleMaxDistance, NavMesh.AllAreas))
                {
                    result = hit.position;
                    Debug.Log($"[Spawn] L1 candidate={candidate} -> hit={hit.position} | center={ctx.Center} min={ctx.MinSpawnRadius} max={ctx.MaxSpawnRadius}");
                    return true;
                }
            }

            // --- Level 2: Centre ---
            if (NavMesh.SamplePosition(ctx.Center, out var centerHit, SampleMaxDistance, NavMesh.AllAreas))
            {
                result = centerHit.position;
                Debug.Log($"[Spawn] L2 (fallback to center) -> {centerHit.position} | center={ctx.Center}");
                return true;
            }

            // --- Level 3: wide search around the centre ---
            var wideRadius = Mathf.Max(ctx.MaxSpawnRadius, SampleMaxDistance) * 4f;
            if (NavMesh.SamplePosition(ctx.Center, out var wideHit, wideRadius, NavMesh.AllAreas))
            {
                result = wideHit.position;
                Debug.Log($"[Spawn] L3 (wide) -> {wideHit.position} | center={ctx.Center}");
                return true;
            }

            result = default;
            return false;
        }
    }
}