using System;
using System.Collections.Generic;
using _Project.Scripts.Domain.Gameplay;
using _Project.Scripts.Domain.Gameplay.Interfaces;
using Cysharp.Threading.Tasks;

namespace _Project.Scripts.Installers.Bootstrap
{
    public class UpgradeChoiceProviderStub : IUpgradeChoiceProvider
    {
        public void ShowUpgrades(List<UpgradeType> choices, Action<UpgradeType> onSelected)
        {
            _ = UniTask.Delay(2000).ContinueWith(() =>
            {
                if (choices.Count > 0) onSelected?.Invoke(choices[0]);
            });
        }
    }
}