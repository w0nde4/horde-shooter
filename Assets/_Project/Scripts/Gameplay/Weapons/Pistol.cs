namespace _Project.Scripts.Gameplay.Weapons
{
    public class Pistol : WeaponBase
    {
        protected override void Fire()
        {
            PerformRaycast();
        }
    }
}