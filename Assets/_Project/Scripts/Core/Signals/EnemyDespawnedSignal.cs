using _Project.Scripts.Core.Interfaces;

namespace _Project.Scripts.Core.Signals
{
    public struct EnemyDespawnedSignal
    {
        public IEnemyHealth Enemy { get; }
        public EnemyDespawnedSignal(IEnemyHealth enemy) => Enemy = enemy;
    }
}