using System;
using _Project.Scripts.Core.Interfaces;
using _Project.Scripts.Core.Signals;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Services.Network
{
    public class NetworkPlayerSpawnHandler : IInitializable, IDisposable
    {
        private readonly IPlayerSpawnService _playerSpawnService;
        private readonly SignalBus _signalBus;

        [Inject]
        public NetworkPlayerSpawnHandler(IPlayerSpawnService playerSpawnService, SignalBus signalBus)
        {
            _playerSpawnService = playerSpawnService;
            _signalBus = signalBus;
        }
        
        public void Initialize()
        {
            _signalBus.Subscribe<PlayerJoinRequestSignal>(OnJoinRequested);
            _signalBus.Subscribe<PlayerLeaveRequestSignal>(OnLeaveRequested);
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<PlayerJoinRequestSignal>(OnJoinRequested);
            _signalBus.Unsubscribe<PlayerLeaveRequestSignal>(OnLeaveRequested);
        }

        private void OnJoinRequested(PlayerJoinRequestSignal signal)
        {
            _playerSpawnService
                .SpawnPlayerAsync(signal.OwnerID, Vector3.zero, Quaternion.identity, signal.PlayerRefRaw)
                .Forget();
        }

        private void OnLeaveRequested(PlayerLeaveRequestSignal signal)
        {
            _playerSpawnService.DespawnPlayer(signal.OwnerID, signal.DestroyObject);
        }
    }
}