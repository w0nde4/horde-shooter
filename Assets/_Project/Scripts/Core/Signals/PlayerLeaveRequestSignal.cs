namespace _Project.Scripts.Core.Signals
{
    public readonly struct PlayerLeaveRequestSignal
    {
        public readonly string OwnerID;
        public readonly bool DestroyObject;

        public PlayerLeaveRequestSignal(string ownerID, bool destroyObject)
        {
            OwnerID = ownerID;
            DestroyObject = destroyObject;
        }
    }
}