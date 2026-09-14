using System;
using _Project.Scripts.Domain.Gameplay.Configs;
using _Project.Scripts.Domain.Gameplay.Data;
using _Project.Scripts.Domain.Gameplay.Interfaces;
using _Project.Scripts.Gameplay.Enemy;
using _Project.Scripts.Services;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Waves
{
    public class WaveSpawner : MonoBehaviour, IWaveSpawner
    {
        [Inject] private WaveSpawnerConfig _config;
        [Inject] private PlayerRegistry _playerRegistry;
        [Inject] private IEnemySpawnService _enemySpawnService;

        private int _aliveEnemies;
        
        public event Action AllEnemiesDefeated;

        private void Start()
        {
            if (_config != null) return;
            Debug.LogError("WaveSpawner: WaveSpawnerConfig is not assigned!");
        }

        public void StartWave(WaveConfig wave)
        {
            if (wave == null)
            {
                Debug.LogError("WaveSpawner: WaveConfig is null");
                return;
            }

            _aliveEnemies = 0;
            SpawnWaveAsync(wave).Forget();
        }

        private async UniTaskVoid SpawnWaveAsync(WaveConfig wave)
        {
            var spawned = 0;
            while (spawned < wave.enemyCount)
            {
                if (_aliveEnemies >= _config.maxAliveEnemies)
                {
                    await UniTask.Yield();
                    continue;
                }

                var success = await SpawnEnemyAsync(wave);
                if (success)
                {
                    spawned++;
                    await UniTask.Delay(TimeSpan.FromSeconds(wave.spawnInterval), cancellationToken: this.GetCancellationTokenOnDestroy());
                }
                else
                {
                    Debug.LogWarning("WaveSpawner: Spawn failed, retrying in 0.5 seconds..."); //todo: enemy climbs any height, spawns independently from player
                    await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: this.GetCancellationTokenOnDestroy());
                }
            }

            await UniTask.WaitUntil(() => _aliveEnemies == 0, cancellationToken: this.GetCancellationTokenOnDestroy());
            Debug.Log($"Wave {wave.waveNumber}: all enemies defeated.");
            AllEnemiesDefeated?.Invoke();
        }

        private async UniTask<bool> SpawnEnemyAsync(WaveConfig wave)
        {
            var ctx = new EnemySpawnContext
            {
                PrefabAddress = _config.enemyPrefabAddress,
                Center = GetNearestPlayerPosition(),
                PlayersTransforms = GetPlayersTransforms(),
                MinSpawnRadius = _config.minSpawnRadius,
                MaxSpawnRadius = _config.maxSpawnRadius,
                Speed = wave.enemySpeed,
                PlayerLayer = _config.playerLayer,
                OnSetupObject = (enemyGameObject) =>
                {
                    if (enemyGameObject.TryGetComponent<EnemyBrain>(out var brain))
                    {
                        brain.SetSpeed(wave.enemySpeed);
                    }
                }
            };
            
            var enemyObj = await _enemySpawnService.SpawnEnemyAsync(ctx);
            if (enemyObj == null) return false;
            
            var health = enemyObj.GetComponent<EnemyHealth>();
            if (health != null)
            {
                _aliveEnemies++;
                health.OnDeath += OnEnemyDeath;
                return true;
            }
            
            Debug.LogError("Enemy prefab missing EnemyHealth component!");
            Destroy(enemyObj);
            return false;
        }

        private Transform[] GetPlayersTransforms()
        {
            var players = _playerRegistry.Players;
            if (players == null || players.Count == 0) 
                return Array.Empty<Transform>();

            var valid = new System.Collections.Generic.List<Transform>(players.Count);
            foreach (var p in players)
            {
                if (p != null) valid.Add(p.transform);
            }
            return valid.ToArray();
        }
        
        private Vector3 GetNearestPlayerPosition()
        {
            var players = GetPlayersTransforms();
            if (players.Length == 0) 
                return transform.position; // fallback

            var nearestPos = players[0].position;
            var minDist = float.MaxValue;
            foreach (var p in players)
            {
                if (p == null) continue;
                var d = Vector3.Distance(transform.position, p.position);
                if (d < minDist)
                {
                    minDist = d;
                    nearestPos = p.position;
                }
            }
            return nearestPos;
        }

        private void OnEnemyDeath(EnemyHealth enemy)
        {
            _aliveEnemies--;
            enemy.OnDeath -= OnEnemyDeath;
        }
    }
}