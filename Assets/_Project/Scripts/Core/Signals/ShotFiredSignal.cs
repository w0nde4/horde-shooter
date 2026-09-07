using _Project.Scripts.Core.Data;

namespace _Project.Scripts.Core.Signals
{
    public readonly struct ShotFiredSignal
    {
        public ShotResult Shot { get; }

        public ShotFiredSignal(ShotResult shot)
        {
            Shot = shot;
        }
    }
}