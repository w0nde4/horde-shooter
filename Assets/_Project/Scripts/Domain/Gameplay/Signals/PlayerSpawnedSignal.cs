using UnityEngine;

namespace _Project.Scripts.Domain.Gameplay.Signals
{
    public readonly struct PlayerSpawnedSignal
    {
        public readonly GameObject Player;
        public readonly PlayerStats PlayerStats;
        public readonly string OwnerID;

        public PlayerSpawnedSignal(GameObject player, PlayerStats playerStats, string ownerID)
        {
            Player = player;
            PlayerStats = playerStats;
            OwnerID = ownerID;
        }
    }
}