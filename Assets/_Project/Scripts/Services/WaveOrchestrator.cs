using System;
using System.Collections.Generic;
using System.Threading;
using _Project.Scripts.Core.Data;
using _Project.Scripts.Core.Signals;
using _Project.Scripts.Domain.Gameplay.Configs;
using _Project.Scripts.Domain.Gameplay.Interfaces;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Services
{
    public class WaveOrchestrator : IWaveOrchestrator, IDisposable
    {
        private readonly WaveCycleConfig _config;
        private readonly IWaveSpawner _waveSpawner;
        private readonly PlayerRegistry _playerRegistry;
        private readonly SignalBus _signalBus;
        
        private CancellationTokenSource _waveCycleCts;
        private int _currentWaveIndex;
        private bool _isWaveActive;
        
        public event Action<WaveConfig> OnWaveStart;
        public event Action OnWaveComplete;

        [Inject]
        public WaveOrchestrator(WaveCycleConfig config, IWaveSpawner waveSpawner, PlayerRegistry playerRegistry, SignalBus signalBus)
        {
            _config = config;
            _waveSpawner = waveSpawner;
            _playerRegistry = playerRegistry;
            _signalBus = signalBus;
            _waveCycleCts = new CancellationTokenSource();
        }
        
        public async UniTask StartWaveCycle(CancellationToken ct)
        {
            if (_config == null || _config.waves == null || _config.waves.Length == 0)
            {
                Debug.LogWarning("StartWaveCycle: no waves configured.");
                return;
            }
            
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, _waveCycleCts.Token);
            var token = linkedCts.Token;

            _currentWaveIndex = 0;
            while (_currentWaveIndex < _config.waves.Length && !ct.IsCancellationRequested)
            {
                var wave = _config.waves[_currentWaveIndex];

                _signalBus.Fire(new WaveStateChangedSignal(WaveState.Intermission, wave.waveNumber, _config.intermissionDuration));
                await RunIntermission(wave, token);
                if (token.IsCancellationRequested) break;
                
                _signalBus.Fire(new WaveStateChangedSignal(WaveState.Countdown, wave.waveNumber, _config.countdownDuration));
                await RunCountdown(wave, token);
                if (token.IsCancellationRequested) break;
                
                _signalBus.Fire(new WaveStateChangedSignal(WaveState.Fighting, wave.waveNumber));
                await RunWave(wave, token);
                if (token.IsCancellationRequested) break;
                
                _signalBus.Fire(new WaveStateChangedSignal(WaveState.Upgrade, wave.waveNumber));
                await RunUpgradePhase(token);

                _currentWaveIndex++;
                Debug.Log($"Wave index now: {_currentWaveIndex}");
            }
            
            if (ct.IsCancellationRequested)
                return;

            _signalBus.Fire(new WaveStateChangedSignal(WaveState.Victory, _config.waves.Length));
            Debug.Log("All waves completed!");
            _signalBus.Fire<AllWavesCompletedSignal>();
        }

        private async UniTask RunIntermission(WaveConfig wave, CancellationToken ct)
        {
            Debug.Log($"Wave {wave.waveNumber}: Intermission {_config.intermissionDuration} seconds...");
            await UniTask.Delay(TimeSpan.FromSeconds(_config.intermissionDuration), cancellationToken: ct);
        }

        private async UniTask RunCountdown(WaveConfig wave, CancellationToken ct)
        {
            Debug.Log($"Wave {wave.waveNumber}: Countdown {_config.countdownDuration} seconds...");
            await UniTask.Delay(TimeSpan.FromSeconds(_config.countdownDuration), cancellationToken: ct);
        }

        private async UniTask RunWave(WaveConfig wave, CancellationToken ct)
        {
            _isWaveActive = true;
            OnWaveStart?.Invoke(wave);
            _waveSpawner.StartWave(wave);

            await UniTask.WaitUntil(() => !_isWaveActive, cancellationToken: ct);

            OnWaveComplete?.Invoke();
            Debug.Log($"Wave {wave.waveNumber} completed!");
        }

        private async UniTask RunUpgradePhase(CancellationToken ct)
        {
            var activeManagers = new List<IUpgradeManager>();
            foreach (var playerObj in _playerRegistry.Players)
            {
                if (playerObj == null) continue;
                if (playerObj.TryGetComponent<IUpgradeManager>(out var manager))
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
        }
        
        public void ResetOrchestrator()
        {
            _waveCycleCts.Cancel();
            _waveCycleCts.Dispose();
            _waveCycleCts = new CancellationTokenSource();
            
            _currentWaveIndex = 0;
            _isWaveActive = false;
        }

        public void Dispose()
        {
            _waveCycleCts?.Cancel();
            _waveCycleCts?.Dispose();
            _waveCycleCts = null;
        }

        public CancellationToken GetToken() => _waveCycleCts?.Token ?? CancellationToken.None;

        public void OnAllEnemiesDefeated() => _isWaveActive = false;
    }
}