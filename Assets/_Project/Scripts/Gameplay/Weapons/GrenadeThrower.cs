using _Project.Scripts.Core.Data;
using _Project.Scripts.Core.Interfaces;
using _Project.Scripts.Domain.Gameplay;
using _Project.Scripts.Domain.Gameplay.Configs;
using _Project.Scripts.Domain.Gameplay.Data;
using Fusion;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Weapons
{
    public class GrenadeThrower : NetworkBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private GrenadeConfig grenadeConfig;
        [SerializeField] private Transform throwOrigin;
        
        [SerializeField] private NetworkObject networkGrenadePrefab;

        [Inject] private PlayerStats _playerStats;
        [Inject] private ISpawnService _spawnService;
        
        [Networked] private NetworkButtons _oldButtons { get; set; }
        [Networked] private float _lastThrowTime { get; set; } = -1000f;
        
        private PlayerIntent _intent;
        private float _lastThrowTimeOffline = -Mathf.Infinity;
        
        public PlayerIntent Intent
        {
            get => _intent;
            set => _intent = value; 
        }

        private void Start()
        {
            if (throwOrigin == null)
                Debug.LogError("GrenadeThrower: Throw origin is not assigned!");
        }

        public override void FixedUpdateNetwork()
        {
            if (GetInput(out NetworkInputData input))
            {
                var pressed = input.Buttons.GetPressed(_oldButtons);
                var grenadePressedThisTick = pressed.IsSet(PlayerButtons.Grenade);
                
                if (grenadePressedThisTick && CanThrowNetwork())
                {
                    if (HasStateAuthority)
                    {
                        ThrowGrenadeNetwork();
                    }
                    else
                    {
                        Rpc_RequestGrenadeThrow();
                    }
                }

                _oldButtons = input.Buttons;
            }
        }

        private void Update()
        {
            if (Runner == null || !Runner.IsRunning)
            {
                if (_intent.Grenade && CanThrowOffline())
                {
                    ThrowGrenadeOffline();
                    _intent.Grenade = false;
                }
            }
        }
        
        private bool CanThrowNetwork()
        {
            if (grenadeConfig == null || _playerStats == null) return false;
            
            var cooldown = grenadeConfig.throwCooldown * _playerStats.GrenadeCooldownMultiplier;
            return Runner.SimulationTime >= _lastThrowTime + cooldown;
        }

        private bool CanThrowOffline()
        {
            if (grenadeConfig == null || _playerStats == null) return false;
            
            var cooldown = grenadeConfig.throwCooldown * _playerStats.GrenadeCooldownMultiplier;
            return Time.time >= _lastThrowTimeOffline + cooldown;
        }

        private void ThrowGrenadeNetwork()
        {
            if (!HasStateAuthority) return;

            if (networkGrenadePrefab == null)
            {
                Debug.LogError("GrenadeThrower: networkGrenadePrefab не назначен в инспекторе!");
                return;
            }

            Debug.Log("Trying to spawn network grenade");
            var grenadeObj = Runner.Spawn(
                networkGrenadePrefab,
                throwOrigin.position,
                throwOrigin.rotation,
                Object.InputAuthority
            );

            if (grenadeObj != null)
            {
                Debug.Log("Grenade spawned");
                grenadeObj.gameObject.SetActive(true);
                InitializeAndPush(grenadeObj.gameObject);
            }

            _lastThrowTime = Runner.SimulationTime;
        }
        
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void Rpc_RequestGrenadeThrow()
        {
            ThrowGrenadeNetwork();
        }
        
        private async void ThrowGrenadeOffline()
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
            grenadeObject.gameObject.SetActive(true);
                
            var rb = grenadeObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                var force = throwOrigin.forward * grenadeConfig.throwForce + Vector3.up * grenadeConfig.upwardForce;
                rb.AddForce(force, ForceMode.Impulse);
            }
        
            _lastThrowTimeOffline = Time.time;
        }
        
        private void InitializeAndPush(GameObject grenadeObject)
        {
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
        }
        
        public override void Spawned()
        {
            if (Runner.IsServer)
            {
                _lastThrowTime = -1000f;
            }
        }
    }
}