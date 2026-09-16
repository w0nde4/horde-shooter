using System;
using System.Collections.Generic;
using System.Threading;
using _Project.Scripts.Domain.Gameplay.Configs;
using _Project.Scripts.Domain.Gameplay.Interfaces;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Services
{
    public class WaveOrchestrator : IWaveOrchestrator
    {
        private readonly WaveCycleConfig _config;
        private readonly IWaveSpawner _waveSpawner;
        private readonly PlayerRegistry _playerRegistry;
        private int _currentWaveIndex;
        private bool _isWaveActive;
        
        public event Action<WaveConfig> OnWaveStart;
        public event Action OnWaveComplete;

        [Inject]
        public WaveOrchestrator(WaveCycleConfig config, IWaveSpawner waveSpawner, PlayerRegistry playerRegistry)
        {
            _config = config;
            _waveSpawner = waveSpawner;
            _playerRegistry = playerRegistry;
        }
        
        public async UniTask StartWaveCycle(CancellationToken ct)
        {
            _currentWaveIndex = 0;
            while (_currentWaveIndex < _config.waves.Length && !ct.IsCancellationRequested)
            {
                var wave = _config.waves[_currentWaveIndex];

                Debug.Log($"Wave {wave.waveNumber}: Intermission {_config.intermissionDuration} seconds...");
                await UniTask.Delay(TimeSpan.FromSeconds(_config.intermissionDuration), cancellationToken: ct);

                Debug.Log($"Wave {wave.waveNumber}: Countdown {_config.countdownDuration} seconds...");
                await UniTask.Delay(TimeSpan.FromSeconds(_config.countdownDuration), cancellationToken: ct);

                _isWaveActive = true;
                OnWaveStart?.Invoke(wave);
                _waveSpawner.StartWave(wave);

                await UniTask.WaitUntil(() => !_isWaveActive, cancellationToken: ct);

                OnWaveComplete?.Invoke();
                Debug.Log($"Wave {wave.waveNumber} completed!");

                var activeManagers = new List<IUpgradeManager>();
                foreach (var playerObj in _playerRegistry.Players)
                {
                    if (playerObj == null) continue;
                    var manager = playerObj.GetComponent<IUpgradeManager>();
                    if (manager != null)
                    {
                        activeManagers.Add(manager);
                    }
                }
                
                Debug.Log($"[Server] Opening upgrades for {activeManagers.Count} players.");

                foreach (var manager in activeManagers)
                {
                    manager.ShowUpgrades();
                }

                var waitTasks = activeManagers.Select(manager =>
                    UniTask.WaitUntil(() => manager.IsUpgradeSelected, cancellationToken: ct)
                );
                await UniTask.WhenAll(waitTasks);

                foreach (var manager in activeManagers)
                {
                    manager.ResetSelection();
                }
                
                _currentWaveIndex++;
                Debug.Log($"Wave index now: {_currentWaveIndex}");
            }

            Debug.Log("All waves completed!");
        }

        public void OnAllEnemiesDefeated() => _isWaveActive = false;
    }
}