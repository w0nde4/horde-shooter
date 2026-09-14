using _Project.Scripts.Core.Interfaces;
using _Project.Scripts.Domain.Gameplay.Configs;
using _Project.Scripts.Domain.Gameplay.Interfaces;
using _Project.Scripts.Gameplay.Player;
using _Project.Scripts.Gameplay.Upgrades;
using _Project.Scripts.Gameplay.Waves;
using _Project.Scripts.Services;
using _Project.Scripts.Services.Network;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Installers
{
    public class GameInstaller : MonoInstaller
    {
        [SerializeField] private WaveCycleConfig waveCycleConfig;
        [SerializeField] private WaveSpawnerConfig waveSpawnerConfig;
        [SerializeField] private PlayerConfig playerConfig;
        [SerializeField] private bool useNetwork;

        public override void InstallBindings()
        {
            // === Configs ===
            Container.Bind<WaveCycleConfig>()
                .FromInstance(waveCycleConfig)
                .AsSingle();
            Container.Bind<WaveSpawnerConfig>()
                .FromInstance(waveSpawnerConfig)
                .AsSingle();
            Container.Bind<PlayerConfig>()
                .FromInstance(playerConfig)
                .AsSingle();
            
            // === Network check ===
            if (useNetwork)
            {
                Container.Bind<IPlayerSpawnService>()
                    .To<NetworkPlayerSpawnService>()
                    .AsSingle();
                Container.Bind<ISpawnService>()
                    .To<FusionSpawnService>()
                    .AsSingle();
            }
            else
            {
                Container.Bind<IPlayerSpawnService>()
                    .To<OfflinePlayerSpawnService>()
                    .AsSingle();
                Container.Bind<ISpawnService>()
                    .To<OfflineSpawnService>()
                    .AsSingle();
            }
            
            // === Player-scoped services ===
            Container.Bind<PlayerRegistry>()
                .AsSingle()
                .NonLazy();
            Container.BindInterfacesAndSelfTo<NetworkPlayerSpawnHandler>()
                .AsSingle()
                .NonLazy();
            
            // === Interfaces for waves/enemies/upgrades ===
            Container.Bind<IWaveOrchestrator>()
                .To<WaveOrchestrator>()
                .AsSingle();
            Container.Bind<IEnemySpawnService>()
                .To<EnemySpawnService>()
                .AsSingle();
            Container.Bind<IWaveSpawner>()
                .To<WaveSpawner>()
                .FromComponentInHierarchy()
                .AsSingle();
            Container.Bind<IUpgradeManager>()
                .To<UpgradeManager>()
                .FromComponentInHierarchy()
                .AsSingle();

            // === MonoBehaviour ===
            Container.Bind<WaveCycle>()
                .FromComponentInHierarchy()
                .AsSingle();
            Container.Bind<PlayerSpawner>()
                .FromComponentInHierarchy()
                .AsSingle();
        }
    }
}