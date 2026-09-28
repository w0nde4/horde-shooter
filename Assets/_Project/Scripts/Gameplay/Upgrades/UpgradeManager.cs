using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Core.Signals;
using _Project.Scripts.Domain.Gameplay;
using _Project.Scripts.Domain.Gameplay.Interfaces;
using Fusion;
using UnityEngine;
using Zenject;
using Random = System.Random;

namespace _Project.Scripts.Gameplay.Upgrades
{
    public class UpgradeManager : NetworkBehaviour, IUpgradeManager
    {
        [Header("Upgrade Choices")]
        [SerializeField] private int minChoices = 3;
        [SerializeField] private int maxChoices = 4;

        [Inject] private IUpgradeChoiceProvider _choiceProvider;
        [Inject] private SignalBus _signalBus;
    
        private PlayerStats _playerStats;
        public event Action<UpgradeType> UpgradeSelected;
        
        [Networked, Capacity(4)]
        private NetworkArray<UpgradeType> _networkChoices => default;

        [Networked, Capacity(16), OnChangedRender(nameof(OnStatsNetworkChanged))] 
        public NetworkArray<float> NetworkedStats => default;
        
        [Networked, OnChangedRender(nameof(OnUpgradeStateChanged))]
        public NetworkBool IsUpgradeSelected { get; private set; }
        
        public PlayerStats PlayerStatsInstance => _playerStats;

        [Inject]
        private void Construct(PlayerStats playerStats)
        {
            _playerStats = playerStats;
        }

        public override void Spawned()
        {
            base.Spawned();

            if (HasStateAuthority)
            {
                IsUpgradeSelected = true;
            }
            
            if (_playerStats != null)
            {
                _playerStats.StatResolver = GetStatValue;
                _playerStats.OnUpgradeRequested += HandleUpgradeRequest;
                if (HasStateAuthority)
                {
                    _playerStats.InitializeBaseStats(SetStatValue);
                }
            }
        }
        
        private void HandleUpgradeRequest(UpgradeType type, float newValue)
        {
            if (HasStateAuthority)
            {
                SetStatValue(type, newValue);
            }
        }
        
        public override void Despawned(NetworkRunner runner, bool hasStateAuthority)
        {
            if (_playerStats != null)
            {
                _playerStats.StatResolver = null;
                _playerStats.OnUpgradeRequested -= HandleUpgradeRequest;
            }
        }

        public float GetStatValue(UpgradeType type, float fallback)
        {
            var index = (int)type;
            var networkValue = NetworkedStats[index];
            return networkValue != 0f ? networkValue : fallback;
        }

        public void SetStatValue(UpgradeType type, float newValue)
        {
            if(!HasStateAuthority) return;
            NetworkedStats.Set((int)type, newValue);
        }

        public void ShowUpgrades()
        {
            if(!HasStateAuthority) return;
            
            IsUpgradeSelected = false;

            for (var i = 0; i < _networkChoices.Length; i++)
            {
                _networkChoices.Set(i, default);
            }

            var playerSeed = Environment.TickCount + Object.InputAuthority.PlayerId;
            var rand = new Random(playerSeed);

            var allTypes = ((UpgradeType[])Enum.GetValues(typeof(UpgradeType)))
                .Where(t => t != default)
                .OrderBy(t => rand.Next())
                .ToList();

            var count = UnityEngine.Random.Range(minChoices, maxChoices + 1);

            for (var i = 0; i < Mathf.Min(count, allTypes.Count); i++)
            {
                _networkChoices.Set(i, allTypes[i]);
            }
        }

        private void OnUpgradeStateChanged()
        {
            if (!HasInputAuthority) return;

            if (!IsUpgradeSelected)
            {
                var choiceList = new List<UpgradeType>();
                
                foreach (var choice in _networkChoices)
                {
                    if (choice != default)
                    {
                        choiceList.Add(choice);
                    }
                }
                
                if(choiceList.Count == 0) return;
                
                _signalBus.Fire<UpgradeSelectionStartedSignal>();
                _choiceProvider.ShowUpgrades(choiceList, SelectUpgrade);
            }
            
            else
            {
                _signalBus.Fire<UpgradeSelectionEndedSignal>();
            }
        }
        
        private void OnStatsNetworkChanged()
        {
            if (HasInputAuthority && _playerStats != null)
            {
                Debug.Log("[Client] My networked stats updated!");
                _playerStats.RefreshLocalUI();
            }
        }

        public void SelectUpgrade(UpgradeType type)
        {
            if (IsUpgradeSelected) return;
            Rpc_RequestUpgrade(type);
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void Rpc_RequestUpgrade(UpgradeType type)
        {
            if (_playerStats == null) return;
            if (IsUpgradeSelected) return;

            var isValidChoice = false;
            var requestedTypeIndex = (int)type;
            for (var i = 0; i < _networkChoices.Length; i++)
            {
                var choiceIndex = (int)_networkChoices[i];
                if (choiceIndex == requestedTypeIndex && type != default)
                {
                    isValidChoice = true;
                    break;
                }
            }

            if (!isValidChoice)
            {
                var currentOptions = string.Join(", ", _networkChoices);
                Debug.LogWarning($"[Server] Rejecting upgrade request! Player {Object.InputAuthority} tried to select {type}. Current choices in network state: [{currentOptions}]");
                return; 
            }
            
            IsUpgradeSelected = true;
            
            _playerStats.ApplyUpgrade(type);
            UpgradeSelected?.Invoke(type);
            
            Debug.Log($"[Server] Successfully applied upgrade {type} to player {Object.InputAuthority}");
        }

        public void ResetSelection()
        {
            if(!HasStateAuthority) return;
            
            IsUpgradeSelected = true;

            for (var i = 0; i < _networkChoices.Length; i++)
            {
                _networkChoices.Set(i, default);
            }
        }
    }
}