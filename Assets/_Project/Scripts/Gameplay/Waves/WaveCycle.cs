using System;
using _Project.Scripts.Domain.Gameplay.Configs;
using _Project.Scripts.Domain.Gameplay.Interfaces;
using _Project.Scripts.Services;
using _Project.Scripts.Services.Network;
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
        [Inject] private FusionNetworkService _networkService;
        
        private bool _cycleStarted;

        public bool IsAuthoritative =>
            _networkService == null || !_networkService.IsRunning || _networkService.IsServer;
        
        public event Action<WaveConfig> OnWaveStart;
        public event Action OnWaveComplete;

        public void Awake()
        {
            _playerRegistry.OnPlayerRegistered += OnPlayerRegistered;
            if (_playerRegistry.HasPlayer && !_cycleStarted && IsAuthoritative)
            {
                _cycleStarted = true;
                StartCycle();
            }
        }
        
        private void OnPlayerRegistered(GameObject player)
        {
            if(_cycleStarted || !IsAuthoritative) return;
            _cycleStarted = true;
            StartCycle();
        }
        
        private void StartCycle()
        {
            _orchestrator.OnWaveStart += OnWaveStart;
            _orchestrator.OnWaveComplete += OnWaveComplete;
            _orchestrator.StartWaveCycle(this.GetCancellationTokenOnDestroy()).Forget();
        }

        private void Start()
        {
            if (_waveSpawner != null) _waveSpawner.AllEnemiesDefeated += OnAllEnemiesDefeated;
        }

        private void OnAllEnemiesDefeated() => _orchestrator.OnAllEnemiesDefeated();

        private void OnDestroy()
        {
            if (_waveSpawner != null) _waveSpawner.AllEnemiesDefeated -= OnAllEnemiesDefeated;
            _playerRegistry.OnPlayerRegistered -= OnPlayerRegistered;
            _orchestrator.OnWaveStart -= OnWaveStart;
            _orchestrator.OnWaveComplete -= OnWaveComplete;
        }
    }
}