using System;
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
        private readonly IUpgradeManager _upgradeManager;
        private int _currentWaveIndex;
        private bool _isWaveActive;
        
        public event Action<WaveConfig> OnWaveStart;
        public event Action OnWaveComplete;

        [Inject]
        public WaveOrchestrator(WaveCycleConfig config, IWaveSpawner waveSpawner, IUpgradeManager upgradeManager)
        {
            _config = config;
            _waveSpawner = waveSpawner;
            _upgradeManager = upgradeManager;
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

                _upgradeManager.ShowUpgrades();
                await UniTask.WaitUntil(() => _upgradeManager.IsUpgradeSelected, cancellationToken: ct);
                _upgradeManager.ResetSelection();

                _currentWaveIndex++;
                Debug.Log($"Wave index now: {_currentWaveIndex}");
            }

            Debug.Log("All waves completed!");
        }

        public void OnAllEnemiesDefeated() => _isWaveActive = false;
    }
}