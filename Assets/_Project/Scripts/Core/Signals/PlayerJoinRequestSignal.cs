using Fusion;

namespace _Project.Scripts.Core.Signals
{
    public readonly struct PlayerJoinRequestSignal
    {
        public readonly string OwnerID;
        public readonly int PlayerRefRaw;

        public PlayerJoinRequestSignal(string ownerID, int playerRefRaw)
        {
            OwnerID = ownerID;
            PlayerRefRaw = playerRefRaw;
        }
    }
}