using UnityEngine;

namespace _Project.Scripts.Domain.Gameplay
{
    public enum UpgradeType
    {
        [InspectorName(null)] None = 0,
        Damage,
        MoveSpeed,
        MaxHealth,
        GrenadeCooldown
    }
}