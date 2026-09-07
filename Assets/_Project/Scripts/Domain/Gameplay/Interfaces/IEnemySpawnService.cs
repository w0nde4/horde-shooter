using _Project.Scripts.Domain.Gameplay.Data;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Project.Scripts.Domain.Gameplay.Interfaces
{
    public interface IEnemySpawnService
    {
        UniTask<GameObject> SpawnEnemyAsync(EnemySpawnContext ctx);
    }
}