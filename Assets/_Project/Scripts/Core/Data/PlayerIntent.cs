using UnityEngine;

namespace _Project.Scripts.Core.Data
{
    public struct PlayerIntent
    {
        public Vector2 Move;
        public Vector2 Look;
        public bool Fire;
        public bool Grenade;
        public bool Reload;
        public int WeaponSwitchIndex; // -1 = no switch
        public bool Jump;
        public bool Sprint;
    }
}