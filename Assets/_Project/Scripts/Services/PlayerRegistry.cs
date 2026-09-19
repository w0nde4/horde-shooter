using System;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace _Project.Scripts.Services
{
    public class PlayerRegistry
    {
        private readonly Dictionary<string, GameObject> _players = new();
        private readonly List<GameObject> _playersList = new();
        
        public event Action<GameObject> OnPlayerRegistered;
        public event Action<GameObject> OnPlayerUnregistered;
        
        public IReadOnlyList<GameObject> Players => _playersList;
        public bool HasPlayer => _players.Count > 0;
        public int Count => _players.Count;
        
        public void RegisterPlayer(GameObject player, string id = null)
        {
            if (player == null) return;
            
            if (string.IsNullOrEmpty(id))
            {
                id = Guid.NewGuid().ToString();
            }

            if (_players.ContainsKey(id))
            {
                Debug.LogWarning($"PlayerRegistry: Player with ID {id} is already registered. Overwriting.");
                UnregisterPlayer(id);
            }
            
            _players[id] = player;
            _playersList.Add(player);
            OnPlayerRegistered?.Invoke(player);
        }

        public void UnregisterPlayer(GameObject player)
        {
            if (player == null) return;

            string id = null;
            foreach (var kvp in _players)
            {
                if (kvp.Value == player)
                {
                    id = kvp.Key;
                    break;
                }
            }
            if (id != null) UnregisterPlayer(id);
        }

        public void UnregisterPlayer(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            if(!_players.TryGetValue(id, out var player)) return;
            
            _players.Remove(id);
            _playersList.Remove(player);
            OnPlayerUnregistered?.Invoke(player);
        }
        
        public GameObject GetPlayer(string id)
        {
            _players.TryGetValue(id, out var player);
            return player;
        }
        
        public bool TryGetPlayer(string id, out GameObject player)
        {
            return _players.TryGetValue(id, out player);
        }

        public void Clear()
        {
            _players.Clear();
            _playersList.Clear();
        }
        
        public GameObject GetPlayer(PlayerRef playerRef)
        {
            if (playerRef == PlayerRef.None) return null;

            for (var i = 0; i < _playersList.Count; i++)
            {
                var playerObj = _playersList[i];
                if (playerObj == null) continue;

                if (!playerObj.TryGetComponent<NetworkObject>(out var netObj)) continue;
                
                if (netObj.InputAuthority == playerRef)
                {
                    return playerObj;
                }
            }

            return null;
        }
    }
}