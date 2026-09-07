using _Project.Scripts.Core.Interfaces;
using _Project.Scripts.Services.Network;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Player
{
    public class PlayerSpawner : MonoBehaviour
    {
        [SerializeField] private Transform spawnPoint;

        [Inject] private IPlayerSpawnService _playerSpawnService;
        [Inject] private FusionNetworkService _networkService;

        //непонятно зачем этот класс вообще сейчас нужен
        // private void Start()
        // {
        //     if(_networkService is { IsRunning: true }) return;
        //     
        //     SpawnLocalPlayerAsync().Forget();
        // }

    private async UniTask SpawnLocalPlayerAsync()
        {
            var player = await _playerSpawnService.SpawnPlayerAsync(
                "local",
                spawnPoint != null ? spawnPoint.position : transform.position,
                spawnPoint != null ? spawnPoint.rotation : transform.rotation
                );
            
            if (player == null)
                Debug.LogError("PlayerSpawner: Failed to spawn player!");
        }
    }
}