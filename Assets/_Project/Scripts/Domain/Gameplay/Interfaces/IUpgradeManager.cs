using System;

namespace _Project.Scripts.Domain.Gameplay.Interfaces
{
    public interface IUpgradeManager
    {
        void ShowUpgrades();
        bool IsUpgradeSelected { get; }
        void ResetSelection();
        event Action<UpgradeType> UpgradeSelected;
    }
}