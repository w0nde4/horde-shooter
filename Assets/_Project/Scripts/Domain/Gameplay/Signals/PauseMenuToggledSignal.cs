namespace _Project.Scripts.Domain.Gameplay.Signals
{
    public readonly struct PauseMenuToggledSignal
    {
        public bool IsOpen { get; }
        public PauseMenuToggledSignal(bool isOpen) => IsOpen = isOpen;
    }
}