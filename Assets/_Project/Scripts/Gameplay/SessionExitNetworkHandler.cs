using _Project.Scripts.Core.Signals;
using _Project.Scripts.Services.Network;
using Cysharp.Threading.Tasks;
using Fusion;
using Zenject;

namespace _Project.Scripts.Gameplay
{
    public class SessionExitNetworkHandler : NetworkBehaviour
    {
        [Inject] private SignalBus _signalBus;
        [Inject] private FusionNetworkService _networkService;

        public override void Spawned()
        {
            _signalBus.Subscribe<ExitToMainMenuRequestedSignal>(OnExitRequested);
        }

        public override void Despawned(NetworkRunner runner, bool hasStateAuthority)
        {
            _signalBus.TryUnsubscribe<ExitToMainMenuRequestedSignal>(OnExitRequested);
        }

        private void OnExitRequested()
        {
            if (!_networkService.IsRunning || _networkService.Runner == null) return;

            if (_networkService.Runner.GameMode == GameMode.Single)
            {
                _networkService.ExitToMainMenuAsync(FusionNetworkService.MainMenuSceneName).Forget();
                return;
            }

            if (HasStateAuthority)
            {
                Rpc_ForceReturnToMainMenu();
            }
            else
            {
                _networkService.ExitToMainMenuAsync(FusionNetworkService.MainMenuSceneName).Forget();
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void Rpc_ForceReturnToMainMenu()
        {
            _signalBus.Fire<ReturnToMainMenuForcedSignal>();
        }
    }
}