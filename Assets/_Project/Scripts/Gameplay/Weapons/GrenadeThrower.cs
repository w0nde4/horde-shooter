using System;
using _Project.Scripts.Core.Data;
using _Project.Scripts.Core.Interfaces;
using _Project.Scripts.Domain.Gameplay;
using _Project.Scripts.Domain.Gameplay.Configs;
using _Project.Scripts.Services;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace _Project.Scripts.Gameplay.Weapons
{
    public class GrenadeThrower : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private GrenadeConfig grenadeConfig;
        [SerializeField] private Transform throwOrigin;

        [Inject] private PlayerStats _playerStats;
        [Inject] private ISpawnService _spawnService;
        
        private PlayerIntent _intent;
        private float _lastThrowTime = -Mathf.Infinity;

        private void Start()
        {
            if (throwOrigin == null)
                Debug.LogError("GrenadeThrower: Throw origin is not assigned!");
        }

        private void Update()
        {
            if (_intent.Grenade && CanThrow())
            {
                ThrowGrenade();
                _intent.Grenade = false;
            }
        }
        
        public void OnGrenade(InputValue value)
        {
            if (!CanThrow()) return;
            if (value.isPressed)
                _intent.Grenade = true;
        }

        private bool CanThrow()
        {
            if (grenadeConfig == null || _playerStats == null)
                return false;
            
            var cooldown = grenadeConfig.throwCooldown * _playerStats.GrenadeCooldownMultiplier;
            Debug.Log($"Grenade cooldown: {cooldown}");
            return Time.time >= _lastThrowTime + cooldown;
        }

        private async void ThrowGrenade()
        {
            if (string.IsNullOrEmpty(grenadeConfig.prefabAddress))
            {
                Debug.LogError("GrenadeThrower: Grenade prefab address is not assigned in GrenadeConfig!");
                return;
            }
            
            var grenadeObject = await _spawnService.SpawnAsync(
                grenadeConfig.prefabAddress,
                throwOrigin.position,
                throwOrigin.rotation
            );

            if (grenadeObject == null) return;

            grenadeObject.SetActive(true);
        
            var grenade = grenadeObject.GetComponent<Grenade>();
            if (grenade != null)
                grenade.Initialize(grenadeConfig);
            else
                Debug.LogError("Grenade component missing on prefab!");

            var rb = grenadeObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                var force = throwOrigin.forward * grenadeConfig.throwForce + Vector3.up * grenadeConfig.upwardForce;
                rb.AddForce(force, ForceMode.Impulse);
            }

            _lastThrowTime = Time.time;
        }
    }
}