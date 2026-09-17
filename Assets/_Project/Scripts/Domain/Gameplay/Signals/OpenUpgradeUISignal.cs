using System;
using System.Collections.Generic;

namespace _Project.Scripts.Domain.Gameplay.Signals
{
    public readonly struct OpenUpgradeUISignal
    {
        public List<UpgradeType> Choices { get; }
        public Action<UpgradeType> OnSelected { get; }

        public OpenUpgradeUISignal(List<UpgradeType> choices, Action<UpgradeType> onSelected)
        {
            Choices = choices;
            OnSelected = onSelected;
        }
    }
}