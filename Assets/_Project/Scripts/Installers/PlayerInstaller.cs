using _Project.Scripts.Domain.Gameplay;
using _Project.Scripts.Domain.Gameplay.Configs;
using _Project.Scripts.Domain.Gameplay.Interfaces;
using _Project.Scripts.Gameplay.Player;
using _Project.Scripts.Gameplay.Upgrades;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Installers
{
    public class PlayerInstaller : MonoInstaller
    {
        [SerializeField] private PlayerStatsConfig playerStatsConfig;
        
        public override void InstallBindings()
        {
            Container.Bind<PlayerStats>()
                .AsSingle()
                .NonLazy();
            Container.Bind<PlayerStatsConfig>()
                .FromInstance(playerStatsConfig)
                .AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerHealth>()
                .FromComponentOnRoot()
                .AsSingle();
            Container.Bind<IUpgradeManager>()
                .To<UpgradeManager>()
                .FromComponentOnRoot()
                .AsSingle();
        }
    }
}