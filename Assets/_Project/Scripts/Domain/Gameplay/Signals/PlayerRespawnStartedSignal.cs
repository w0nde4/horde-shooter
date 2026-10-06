using Fusion;

namespace _Project.Scripts.Domain.Gameplay.Signals
{
    public struct PlayerRespawnStartedSignal
    {
        public readonly PlayerRef Player;
        public PlayerRespawnStartedSignal(PlayerRef player) => Player = player;
    }
}