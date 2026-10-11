using _Project.Scripts.Core.Signals;
using Fusion;
using Zenject;

namespace _Project.Scripts.Gameplay
{
    public class VictoryNetworkHandler : NetworkBehaviour
    {
        [Inject] private SignalBus _signalBus;
        
        public override void Spawned()
        {
            if (HasStateAuthority)
            {
                _signalBus.Subscribe<AllWavesCompletedSignal>(OnAllWavesCompleted);
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasStateAuthority)
        {
            if (hasStateAuthority)
            {
                _signalBus.TryUnsubscribe<AllWavesCompletedSignal>(OnAllWavesCompleted);
            }
        }

        private void OnAllWavesCompleted()
        {
            if (!HasStateAuthority) return;
            Rpc_NotifyVictory();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void Rpc_NotifyVictory()
        {
            _signalBus.Fire<VictoryStartedSignal>();
        }
    }
}