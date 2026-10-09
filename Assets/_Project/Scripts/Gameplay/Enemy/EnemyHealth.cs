using System;
using _Project.Scripts.Core.Interfaces;
using _Project.Scripts.Core.Signals;
using Fusion;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Enemy
{
    public class EnemyHealth : NetworkBehaviour, IDamageable, IEnemyHealth
    {
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float despawnDelay = 3f; //todo: move to config

        [Inject] private SignalBus _signalBus;
        
        [Networked]
        private TickTimer DespawnTimer { get; set; }
        
        [Networked]
        [OnChangedRender(nameof(OnHealthNetworkChanged))]
        private float CurrentHealth { get; set; }
        
        [Networked]
        [OnChangedRender(nameof(OnDeathStateChanged))]
        private NetworkBool IsDead { get; set; }
        
        private EnemyBrain _brain;
        private UnityEngine.AI.NavMeshAgent _agent;
        private float _lastRenderHealth;
        
        public float Current => CurrentHealth;
        public float Max => maxHealth;
        public Transform Transform
        {
            get => transform;
        }

        public event Action<float, float> OnHealthChanged;
        public event Action<IEnemyHealth> OnDeath;
    
        private void Awake()
        {
            _brain = GetComponent<EnemyBrain>();
            _agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        }
        
        public override void Spawned()
        {
            _lastRenderHealth = CurrentHealth;
            
            if (HasStateAuthority)
            {
                CurrentHealth = maxHealth;
                IsDead = false;
                DespawnTimer = TickTimer.None;
            }
            
            _signalBus.Fire(new EnemySpawnedSignal(this));
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            _signalBus.Fire(new EnemyDespawnedSignal(this));
            base.Despawned(runner, hasState);
        }

        public void TakeDamage(float damage)
        {
            if(damage <= 0f) return;
            if (HasStateAuthority)
            {
                ApplyDamageInternal(damage);
            }
            else
            {
                RPC_RequestDamage(damage);
            }
        }
        
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_RequestDamage(float damage)
        {
            ApplyDamageInternal(damage);
        }

        private void ApplyDamageInternal(float damage)
        {
            if (IsDead || CurrentHealth <= 0f) return;

            CurrentHealth -= damage;
            Debug.Log($"[Server] Enemy took {damage} damage, remaining: {CurrentHealth}");

            if (CurrentHealth <= 0f)
            {
                CurrentHealth = 0f;
                IsDead = true;
                DespawnTimer = TickTimer.CreateFromSeconds(Runner, despawnDelay);
                ServerPurge();
            }
        }

        private void ServerPurge()
        {
            _brain?.OnDeath();
            if (_agent != null && _agent.isOnNavMesh)
                _agent.isStopped = true;
        }

        public override void FixedUpdateNetwork()
        {
            if(!HasStateAuthority) return;

            if (DespawnTimer.Expired(Runner))
            {
                DespawnTimer = TickTimer.None;
                Runner.Despawn(Object);
            }
        }

        private void OnHealthNetworkChanged()
        {
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
            
            if (CurrentHealth < _lastRenderHealth)
            {
                var damageAmount = _lastRenderHealth - CurrentHealth;

                _signalBus?.Fire(new DamageAppliedSignal(
                    damageAmount, 
                    this, 
                    Vector3.zero, 
                    Vector3.zero, 
                    gameObject
                ));
            }

            _lastRenderHealth = CurrentHealth;
        }
        
        private void OnDeathStateChanged()
        {
            if (IsDead)
            {
                ExecuteLocalDeath();
            }
        }
        
        private void ExecuteLocalDeath()
        {
            Debug.Log("[Client/Host] Enemy death visual triggered.");
            
            OnDeath?.Invoke(this);

            if (_agent != null) 
                _agent.enabled = false;

            var collider = GetComponent<Collider>();
            if (collider != null)
                collider.enabled = false;
        }
    }
}