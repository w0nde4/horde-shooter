using _Project.Scripts.Core.Data;

namespace _Project.Scripts.Core.Signals
{
    public readonly struct WaveStateChangedSignal
    {
        public WaveState State { get; }
        public int WaveNumber { get; }
        public float Duration { get; } //0 если не применимо

        public WaveStateChangedSignal(WaveState state, int waveNumber, float duration = 0f)
        {
            State = state;
            WaveNumber = waveNumber;
            Duration = duration;
        }
    }
}