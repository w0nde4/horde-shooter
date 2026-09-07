using System;
using _Project.Scripts.Core.Signals;
using _Project.Scripts.Domain.Gameplay;
using _Project.Scripts.Domain.Gameplay.Configs;
using _Project.Scripts.Domain.Gameplay.Interfaces;
using _Project.Scripts.Gameplay.Upgrades;
using _Project.Scripts.Services;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Waves
{
    public class WaveCycle : MonoBehaviour
    {
        [Inject] private IWaveOrchestrator _orchestrator;
        [Inject] private IWaveSpawner _waveSpawner;
        [Inject] private SignalBus _signalBus;
        [Inject] private PlayerRegistry _playerRegistry;

        public event Action<WaveConfig> OnWaveStart;
        public event Action OnWaveComplete;

        private bool _cycleStarted;

        public void Awake()
        {
            _playerRegistry.OnPlayerRegistered += OnplayerRegistered;
            //_signalBus.Subscribe<PlayerSpawnedSignal>(OnPlayerSpawned);
            if (_playerRegistry.HasPlayer && !_cycleStarted)
            {
                _cycleStarted = true;
                StartCycle();
            }
        }
        
        private void OnplayerRegistered(GameObject player)
        {
            if(_cycleStarted) return;
            _cycleStarted = true;
            StartCycle();
        }
        
        private void StartCycle()
        {
            _orchestrator.OnWaveStart += OnWaveStart;
            _orchestrator.OnWaveComplete += OnWaveComplete;
            _orchestrator.StartWaveCycle(this.GetCancellationTokenOnDestroy()).Forget();
        }

        private void OnPlayerSpawned()
        {
            if(_cycleStarted) return;
            _cycleStarted = true;
            StartCycle();
        }

        private void Start()
        {
            if (_waveSpawner != null) _waveSpawner.AllEnemiesDefeated += OnAllEnemiesDefeated;
        }

        private void OnAllEnemiesDefeated() => _orchestrator.OnAllEnemiesDefeated();

        private void OnDestroy()
        {
            if (_waveSpawner != null) _waveSpawner.AllEnemiesDefeated -= OnAllEnemiesDefeated;
            _playerRegistry.OnPlayerRegistered -= OnplayerRegistered;
            //_signalBus.TryUnsubscribe<PlayerSpawnedSignal>(OnPlayerSpawned);
            _orchestrator.OnWaveStart -= OnWaveStart;
            _orchestrator.OnWaveComplete -= OnWaveComplete;
        }
    }
}