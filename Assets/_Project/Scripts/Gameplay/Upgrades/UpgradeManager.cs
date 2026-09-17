using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Domain.Gameplay;
using _Project.Scripts.Domain.Gameplay.Interfaces;
using _Project.Scripts.Domain.Gameplay.Signals;
using Fusion;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Upgrades
{
    public class UpgradeManager : NetworkBehaviour, IUpgradeManager
    {
        [Header("Upgrade Choices")]
        [SerializeField] private int minChoices = 3;
        [SerializeField] private int maxChoices = 4;

        [Inject] private IUpgradeChoiceProvider _choiceProvider;
        [Inject] private SignalBus _signalBus;
    
        private List<UpgradeType> _currentChoices = new();
        private PlayerStats _playerStats;
    
        public event Action<UpgradeType> UpgradeSelected;
        public bool IsUpgradeSelected { get; private set; }

        [Inject]
        private void Construct(PlayerStats playerStats)
        {
            _playerStats = playerStats;
        }

        public void ShowUpgrades()
        {
            if(!HasInputAuthority) return;
            
            IsUpgradeSelected = false;
            _currentChoices.Clear();

            var allTypes = (UpgradeType[])Enum.GetValues(typeof(UpgradeType));
            var shuffled = allTypes.OrderBy(x => Guid.NewGuid()).ToList();
            var count = UnityEngine.Random.Range(minChoices, maxChoices + 1);
            _currentChoices = shuffled.Take(count).ToList();

            _choiceProvider.ShowUpgrades(_currentChoices, SelectUpgrade);
        }

        public void SelectUpgrade(UpgradeType type)
        {
            if (IsUpgradeSelected) return;
            
            IsUpgradeSelected = true;

            Rpc_RequestUpgrade(type);
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void Rpc_RequestUpgrade(UpgradeType type)
        {
            if (_playerStats == null) return;
            
            _playerStats.ApplyUpgrade(type);
            UpgradeSelected?.Invoke(type);
            
            Debug.Log($"[Server] Successfully applied upgrade {type} to player {Object.InputAuthority}");
        }

        public void ResetSelection()
        {
            IsUpgradeSelected = false;
            _currentChoices.Clear();
        }
    }
}