using Fusion;

namespace _Project.Scripts.Core.Signals
{
    public readonly struct ReloadStateChangedSignal
    {
        public string WeaponName { get; }
        public PlayerRef InputAuthority { get; }
        public bool IsReloading { get; }

        public ReloadStateChangedSignal(string weaponName, PlayerRef inputAuthority, bool isReloading)
        {
            WeaponName = weaponName;
            InputAuthority = inputAuthority;
            IsReloading = isReloading;
        }
    }
}