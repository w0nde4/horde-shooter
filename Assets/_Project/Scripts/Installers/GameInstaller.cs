using _Project.Scripts.Core.Data;
using _Project.Scripts.Core.Interfaces;
using _Project.Scripts.Domain.Gameplay;
using _Project.Scripts.Domain.Gameplay.Configs;
using _Project.Scripts.Domain.Gameplay.Data;
using _Project.Scripts.Domain.Gameplay.Interfaces;
using _Project.Scripts.Gameplay.Player;
using _Project.Scripts.Gameplay.Upgrades;
using _Project.Scripts.Gameplay.Waves;
using _Project.Scripts.Presentation.Audio;
using _Project.Scripts.Presentation.Configs;
using _Project.Scripts.Presentation.UI;
using _Project.Scripts.Presentation.VFX;
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
        [SerializeField] private WeaponAudioConfig weaponAudioConfig;
        [SerializeField] private WeaponVFXConfig weaponVFXConfig;
        [SerializeField] private Camera respawnCamera;
        [SerializeField] private PlayerSpawnPoint playerSpawnPoint;

        public override void InstallBindings()
        {
            // === Configs ===
            if (waveCycleConfig == null
                || waveSpawnerConfig == null
                || playerConfig == null
                || weaponAudioConfig == null
                || weaponVFXConfig == null)
            {
                Debug.LogError("GameInstaller: Configs is not assigned!");
                return;
            }
            Container.Bind<WaveCycleConfig>()
                .FromInstance(waveCycleConfig)
                .AsSingle();
            Container.Bind<WaveSpawnerConfig>()
                .FromInstance(waveSpawnerConfig)
                .AsSingle();
            Container.Bind<PlayerConfig>()
                .FromInstance(playerConfig)
                .AsSingle();
            Container.Bind<WeaponAudioConfig>()
                .FromInstance(weaponAudioConfig)
                .AsSingle();
            Container.Bind<WeaponVFXConfig>()
                .FromInstance(weaponVFXConfig)
                .AsSingle();

            if (respawnCamera == null)
            {
                Debug.LogError("GameInstaller: Respawn Camera is not assigned!");
                return;
            }
            SetupRespawnCamera();
            Container.Bind<RespawnCamera>()
                .FromInstance(new RespawnCamera(respawnCamera))
                .AsSingle();
            
            if (playerSpawnPoint == null)
            {
                Debug.LogError("GameInstaller: PlayerSpawnPoint is not assigned!");
                return;
            }
            Container.Bind<PlayerSpawnPoint>()
                .FromInstance(playerSpawnPoint)
                .AsSingle()
                .NonLazy();
            
            // === Player-scoped services ===
            Container.Bind<PlayerRegistry>()
                .AsSingle()
                .NonLazy();
            Container.BindInterfacesAndSelfTo<NetworkPlayerSpawnHandler>()
                .AsSingle()
                .NonLazy();
            
            // === Presentation ===
            Container.BindInterfacesAndSelfTo<WeaponFireAudioVisualizer>()
                .AsSingle();
            Container.BindInterfacesAndSelfTo<WeaponReloadAudioVisualizer>()
                .AsSingle();
            Container.BindInterfacesAndSelfTo<WeaponTracerVFXVisualizer>()
                .AsSingle();
            Container.BindInterfacesAndSelfTo<WeaponImpactVFXVisualizer>()
                .AsSingle();

            // === Interfaces ===
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
            Container.Bind<IPlayerSpawnService>()
                .To<NetworkPlayerSpawnService>()
                .AsSingle();
            Container.Bind<ISpawnService>()
                .To<FusionSpawnService>()
                .AsSingle();

            // === MonoBehaviour ===
            Container.Bind<WaveCycle>()
                .FromComponentInHierarchy()
                .AsSingle();
            Container.Bind<IUpgradeChoiceProvider>()
                .To<UpgradeChoiceProvider>()
                .FromComponentInHierarchy()
                .AsSingle();
        }

        private void SetupRespawnCamera()
        {
            respawnCamera.enabled = false;
            var respawnListener = respawnCamera.GetComponent<AudioListener>();
            if (respawnListener != null) respawnListener.enabled = false;
        }
    }
}