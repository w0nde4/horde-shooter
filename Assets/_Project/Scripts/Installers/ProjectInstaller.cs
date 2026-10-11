using _Project.Scripts.Core.Interfaces;
using _Project.Scripts.Core.Signals;
using _Project.Scripts.Domain.Gameplay.Signals;
using _Project.Scripts.Services;
using _Project.Scripts.Services.Network;
using _Project.Scripts.Services.Presentation;
using Zenject;

namespace _Project.Scripts.Installers
{
    public class ProjectInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            // === Global Services ===
            Container.BindInterfacesAndSelfTo<AddressableService>()
                .AsSingle()
                .NonLazy();
            Container.BindInterfacesAndSelfTo<FusionNetworkService>()
                .AsSingle()
                .NonLazy();
            Container.BindInterfacesAndSelfTo<SettingsService>()
                .AsSingle()
                .NonLazy();

            // === Global Interfaces ===
            Container.Bind<IDamageService>()
                .To<CombatService>()
                .AsSingle();
            Container.Bind<IShootingService>()
                .To<ShootingService>()
                .AsSingle();
            Container.Bind<IAudioService>()
                .To<AudioService>()
                .AsSingle();
            Container.Bind<IVFXService>()
                .To<VFXService>()
                .AsSingle();
        
            // === Signals ===
            SignalBusInstaller.Install(Container);
            Container.DeclareSignal<PlayerSpawnedSignal>();
            Container.DeclareSignal<PlayerJoinRequestSignal>();
            Container.DeclareSignal<PlayerLeaveRequestSignal>();
            Container.DeclareSignal<ShotFiredSignal>();
            Container.DeclareSignal<DamageAppliedSignal>();
            Container.DeclareSignal<ReloadStateChangedSignal>();
            Container.DeclareSignal<UpgradeSelectionStartedSignal>();
            Container.DeclareSignal<UpgradeSelectionEndedSignal>();
            Container.DeclareSignal<GameOverStartedSignal>();
            Container.DeclareSignal<PlayerRespawnStartedSignal>();
            Container.DeclareSignal<PlayerRespawnEndedSignal>();
            Container.DeclareSignal<EnemySpawnedSignal>();
            Container.DeclareSignal<EnemyDespawnedSignal>();
            Container.DeclareSignal<GrenadeExplodedSignal>();
            Container.DeclareSignal<PauseMenuToggledSignal>();
            Container.DeclareSignal<ExitToMainMenuRequestedSignal>();
            Container.DeclareSignal<ReturnToMainMenuForcedSignal>();
            Container.DeclareSignal<GameplaySettingsChangedSignal>();
            Container.DeclareSignal<VictoryStartedSignal>();
            Container.DeclareSignal<AllWavesCompletedSignal>();
        }
    }
}