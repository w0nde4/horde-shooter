namespace _Project.Scripts.Gameplay.Weapons
{
    public class Shotgun : WeaponBase
    {
        protected override void Fire()
        {
            for (var i = 0; i < config.pellets; i++)
            {
                PerformRaycast();
            }
        }
    }
}