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
            Container.Bind<AddressableService>()
                .AsSingle()
                .NonLazy();
            Container.BindInterfacesAndSelfTo<FusionNetworkService>()
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
            Container.DeclareSignal<PlayerSpawnedSignal>().OptionalSubscriber();
            Container.DeclareSignal<PlayerJoinRequestSignal>().OptionalSubscriber();
            Container.DeclareSignal<PlayerLeaveRequestSignal>().OptionalSubscriber();
            Container.DeclareSignal<ShotFiredSignal>().OptionalSubscriber();
            Container.DeclareSignal<DamageAppliedSignal>().OptionalSubscriber();
            Container.DeclareSignal<ReloadStateChangedSignal>().OptionalSubscriber();
            Container.DeclareSignal<UpgradeSelectionStartedSignal>().OptionalSubscriber();
            Container.DeclareSignal<UpgradeSelectionEndedSignal>().OptionalSubscriber();
            Container.DeclareSignal<GameOverStartedSignal>().OptionalSubscriber();
            Container.DeclareSignal<PlayerRespawnStartedSignal>().OptionalSubscriber();
            Container.DeclareSignal<PlayerRespawnEndedSignal>().OptionalSubscriber();
        }
    }
}