using Fusion;

namespace _Project.Scripts.Domain.Gameplay.Signals
{
    public struct PlayerRespawnEndedSignal
    {
        public PlayerRef Player;
        public PlayerRespawnEndedSignal(PlayerRef player) => Player = player;
    }
}