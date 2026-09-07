using _Project.Scripts.Domain.Gameplay;
using _Project.Scripts.Gameplay.Player;
using Zenject;

namespace _Project.Scripts.Installers
{
    public class PlayerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<PlayerStats>()
                .AsSingle()
                .NonLazy();
            Container.BindInterfacesAndSelfTo<PlayerHealth>()
                .FromComponentOnRoot()
                .AsSingle();
        }
    }
}