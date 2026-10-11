namespace _Project.Scripts.Core.Signals
{
    public readonly struct GameplaySettingsChangedSignal
    {
        public float Fov { get; }
        public bool InvertY { get; }

        public GameplaySettingsChangedSignal(float fov, bool invertY)
        {
            Fov = fov;
            InvertY = invertY;
        }
    }
}