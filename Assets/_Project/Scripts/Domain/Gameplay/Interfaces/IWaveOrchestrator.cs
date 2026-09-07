using System;
using System.Threading;
using _Project.Scripts.Domain.Gameplay.Configs;
using Cysharp.Threading.Tasks;

namespace _Project.Scripts.Domain.Gameplay.Interfaces
{
    public interface IWaveOrchestrator
    {
        UniTask StartWaveCycle(CancellationToken ct);
        void OnAllEnemiesDefeated();
        event Action<WaveConfig> OnWaveStart;
        event Action OnWaveComplete;
    }
}