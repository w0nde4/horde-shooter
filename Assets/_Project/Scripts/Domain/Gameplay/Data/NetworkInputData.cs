using Fusion;
using UnityEngine;

namespace _Project.Scripts.Domain.Gameplay.Data
{
    public struct NetworkInputData : INetworkInput
    {
        public Vector2 Move;
        public Vector2 Look;
        public int WeaponSwitchIndex;
        public NetworkButtons Buttons;
    }
    
    public enum PlayerButtons
    {
        Jump = 0,
        Sprint = 1,
        Fire = 2,
        Reload = 3,
        Grenade = 4
    }
}