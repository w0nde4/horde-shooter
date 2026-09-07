using System;
using System.Collections.Generic;

namespace _Project.Scripts.Domain.Gameplay.Interfaces
{
    public interface IUpgradeChoiceProvider
    {
        void ShowUpgrades(List<UpgradeType> choices, Action<UpgradeType> onSelected);
    }
}