using _Project.Scripts.Core.Interfaces;

namespace _Project.Scripts.Core.Signals
{
    public readonly struct EnemySpawnedSignal
    {
        public IEnemyHealth Enemy { get; }
        public EnemySpawnedSignal(IEnemyHealth enemy) => Enemy = enemy;
    }
}