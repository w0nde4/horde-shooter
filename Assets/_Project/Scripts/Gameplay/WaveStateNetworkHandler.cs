using _Project.Scripts.Core.Data;
using _Project.Scripts.Core.Signals;
using Fusion;
using Zenject;

namespace _Project.Scripts.Gameplay
{
    public class WaveStateNetworkHandler : NetworkBehaviour
    {
        [Inject] private SignalBus _signalBus;

        public override void Spawned()
        {
            if (HasStateAuthority)
            {
                _signalBus.Subscribe<WaveStateChangedSignal>(OnLocalStateChanged);
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasStateAuthority)
        {
            if (hasStateAuthority)
            {
                _signalBus.TryUnsubscribe<WaveStateChangedSignal>(OnLocalStateChanged);
            }
        }

        private void OnLocalStateChanged(WaveStateChangedSignal signal)
        {
            Rpc_NotifyWaveState(signal.State, signal.WaveNumber, signal.Duration);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void Rpc_NotifyWaveState(WaveState state, int waveNumber, float duration)
        {
            if (HasStateAuthority) return;

            _signalBus.Fire(new WaveStateChangedSignal(state, waveNumber, duration));
        }
    }
}