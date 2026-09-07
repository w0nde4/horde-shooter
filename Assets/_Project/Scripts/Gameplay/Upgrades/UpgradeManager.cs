using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Domain.Gameplay;
using _Project.Scripts.Domain.Gameplay.Interfaces;
using _Project.Scripts.Domain.Gameplay.Signals;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Upgrades
{
    public class UpgradeManager : MonoBehaviour, IUpgradeManager
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
        private void Construct()
        {
            _signalBus.Subscribe<PlayerSpawnedSignal>(OnPlayerSpawned);
        }

        private void OnDestroy()
        {
            _signalBus.TryUnsubscribe<PlayerSpawnedSignal>(OnPlayerSpawned);
        }

        private void OnPlayerSpawned(PlayerSpawnedSignal signal)
        {
            if (signal.PlayerStats == null)
            {
                Debug.LogError("UpgradeManager: recieved PlayerSpawnedSignal wihtout PlayerStats.");
                return;
            }
            
            // TODO(multiplayer): UpgradeManager сейчас держит статы ровно одного игрока
            // (последнего заспавненного). Для честных индивидуальных апгрейдов на каждого
            // сетевого игрока систему нужно переделывать на per-player instance —
            // это часть отдельного этапа "геймплей под мультиплеер", который пока не начат.

            _playerStats = signal.PlayerStats;
        }

        public void ShowUpgrades()
        {
            IsUpgradeSelected = false;
            _currentChoices.Clear();

            var allTypes = (UpgradeType[])Enum.GetValues(typeof(UpgradeType));

            var shuffled = allTypes.OrderBy(x => Guid.NewGuid()).ToList();
            var count = UnityEngine.Random.Range(minChoices, maxChoices + 1);
            _currentChoices = shuffled.Take(count).ToList();

            Debug.Log("Available upgrades:");
            foreach (var type in _currentChoices)
                Debug.Log($"- {type}");
            
            _choiceProvider.ShowUpgrades(_currentChoices, SelectUpgrade);
        }

        public void SelectUpgrade(UpgradeType type)
        {
            if (IsUpgradeSelected) return;
            if (!_currentChoices.Contains(type))
            {
                Debug.LogWarning("Selected upgrade is not in current choices!");
                return;
            }

            IsUpgradeSelected = true;
            _playerStats?.ApplyUpgrade(type);
        
            UpgradeSelected?.Invoke(type);
            Debug.Log($"Player selected upgrade: {type}");
        }

        public void ResetSelection()
        {
            IsUpgradeSelected = false;
            _currentChoices.Clear();
        }
    }
}