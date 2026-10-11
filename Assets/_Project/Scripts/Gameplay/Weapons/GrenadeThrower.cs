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
        
        [Networked] private float LastThrowTime { get; set; } = -1000f;

        private NetworkButtons _oldButtons;
        
        private void Start()
        {
            if (throwOrigin == null)
                Debug.LogError("GrenadeThrower: Throw origin is not assigned!");
        }

        public override void Spawned()
        {
            if (Object.HasStateAuthority)
            {
                LastThrowTime = -1000f;
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (GetInput(out NetworkInputData input))
            {
                var pressed = input.Buttons.GetPressed(_oldButtons);
                var grenadePressedThisTick = pressed.IsSet(PlayerButtons.Grenade);
                
                if (grenadePressedThisTick && CanThrowNetwork())
                {
                    if (Object.HasStateAuthority)
                    {
                        ThrowGrenadeNetwork();
                    }
                }
                
                _oldButtons = input.Buttons;
            }
        }

        private bool CanThrowNetwork()
        {
            if (grenadeConfig == null || _playerStats == null) return false;
            
            var cooldown = grenadeConfig.throwCooldown * _playerStats.GrenadeCooldownMultiplier;
            return Runner.SimulationTime >= LastThrowTime + cooldown;
        }

        private void ThrowGrenadeNetwork()
        {
            if (networkGrenadePrefab == null)
            {
                Debug.LogError("GrenadeThrower: networkGrenadePrefab не назначен в инспекторе!");
                return;
            }
            
            var grenadeObj = Runner.Spawn(
                networkGrenadePrefab,
                throwOrigin.position,
                throwOrigin.rotation,
                Object.InputAuthority
            );

            if (grenadeObj != null)
            {
                grenadeObj.gameObject.SetActive(true);
                InitializeAndPush(grenadeObj.gameObject);
            }

            LastThrowTime = Runner.SimulationTime;
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
    }
}