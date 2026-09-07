using System;
using _Project.Scripts.Domain.Gameplay.Configs;

namespace _Project.Scripts.Domain.Gameplay.Interfaces
{
    public interface IWaveSpawner
    {
        void StartWave(WaveConfig wave);
        event Action AllEnemiesDefeated;
    }
}