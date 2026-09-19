using _Project.Scripts.Core.Data;
using Fusion;
using UnityEngine;

namespace _Project.Scripts.Domain.Gameplay.Signals
{
    public readonly struct ShotFiredSignal
    {
        public ShotResult Shot { get; }
        public string WeaponName { get; }
        public PlayerRef InputAuthority { get; }
        public Vector3 MuzzlePosition { get; }
        public Quaternion MuzzleRotation { get; }

        public ShotFiredSignal(ShotResult shot, string weaponName, PlayerRef inputAuthority, Vector3 muzzlePosition, Quaternion muzzleRotation)
        {
            Shot = shot;
            WeaponName = weaponName;
            InputAuthority = inputAuthority; 
            MuzzlePosition = muzzlePosition;
            MuzzleRotation = muzzleRotation;
        }
    }
}