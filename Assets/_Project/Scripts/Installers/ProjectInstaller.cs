using _Project.Scripts.Core.Interfaces;
using _Project.Scripts.Core.Signals;
using _Project.Scripts.Domain.Gameplay.Interfaces;
using _Project.Scripts.Domain.Gameplay.Signals;
using _Project.Scripts.Installers.Bootstrap;
using _Project.Scripts.Services;
using _Project.Scripts.Services.Network;
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
            Container.Bind<IConfigProvider>()
                .To<ConfigProviderService>()
                .AsSingle();
            Container.Bind<IDamageService>()
                .To<CombatService>()
                .AsSingle();
            Container.Bind<IShootingService>()
                .To<ShootingService>()
                .AsSingle();
            Container.Bind<IUpgradeChoiceProvider>()
                .To<UpgradeChoiceProviderStub>()
                .AsSingle();

            // === Loader ===
            Container.BindInterfacesAndSelfTo<BootstrapLoader>()
                .AsSingle()
                .NonLazy();
        
            // === Signals ===
            SignalBusInstaller.Install(Container);
            Container.DeclareSignal<PlayerSpawnedSignal>().OptionalSubscriber();
            Container.DeclareSignal<PlayerJoinRequestSignal>().OptionalSubscriber();
            Container.DeclareSignal<PlayerLeaveRequestSignal>().OptionalSubscriber();
            Container.DeclareSignal<ShotFiredSignal>().OptionalSubscriber();
            Container.DeclareSignal<DamageAppliedSignal>().OptionalSubscriber();
            Container.DeclareSignal<OpenUpgradeUISignal>().OptionalSubscriber();
        }
    }
}