using System;
using Fusion;

namespace _Project.Scripts.Domain.Gameplay.Interfaces
{
    public interface IUpgradeManager
    {
        void ShowUpgrades();
        NetworkBool IsUpgradeSelected { get; }
        void ResetSelection();
        event Action<UpgradeType> UpgradeSelected;
    }
}