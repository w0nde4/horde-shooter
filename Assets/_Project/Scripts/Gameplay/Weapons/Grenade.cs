using System;
using _Project.Scripts.Core.Interfaces;
using _Project.Scripts.Core.Signals;
using _Project.Scripts.Domain.Gameplay.Configs;
using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Weapons
{
    public class Grenade : NetworkBehaviour
    {
        private const float DespawnDelaySeconds = 0.2f;
        
        [SerializeField] private int maxHitsBuffer = 256;
        
        [Inject] private IDamageService _damageService;
        [Inject] private SignalBus _signalBus;

        private GrenadeConfig _config;
        private bool _exploded;
        private Collider[] _hitsBuffer;
        
        [Networked] private TickTimer DespawnTimer { get; set; }
        [Networked] private TickTimer ExplosionTimer { get; set; }
        
        public void Initialize(GrenadeConfig config)
        { 
            _config = config;
            
            if (Runner != null && Runner.IsRunning)
            {
                if (HasStateAuthority && !ExplosionTimer.IsRunning)
                {
                    ExplosionTimer = TickTimer.CreateFromSeconds(Runner, _config.explosionDelay);
                }
            }
            else
            {
                ExplosionTimerAsync().Forget();
            }
        }

        public override void Spawned()
        {
            Debug.Log($"[GRENADE SPAWNED] IsServer={Runner.IsServer} " +
                      $"HasStateAuthority={HasStateAuthority} " +
                      $"HasInputAuthority={Object.HasInputAuthority} " +
                      $"Id={Object.Id} PrefabId={Object.NetworkTypeId}");
            
            _hitsBuffer = new Collider[maxHitsBuffer];
        }

        private async UniTask ExplosionTimerAsync()
        {
            await UniTask.Delay(
                TimeSpan.FromSeconds(_config.explosionDelay),
                cancellationToken: this.GetCancellationTokenOnDestroy()
            );
            
            if(Runner == null || !Runner.IsRunning) Explode();
        }

        public override void FixedUpdateNetwork()
        {
            if (HasStateAuthority && !_exploded &&  ExplosionTimer.Expired(Runner))
            {
                ExplosionTimer = TickTimer.None;
                Explode();
            }
            
            if (HasStateAuthority && _exploded && DespawnTimer.Expired(Runner))
            {
                DespawnTimer = TickTimer.None;
                Runner.Despawn(Object);
            }
        }

        private void Explode()
        {
            if (_exploded) return;
            _exploded = true;

            if (_config == null)
            {
                Debug.LogError("Grenade: Config missing on Explode!");
                return;
            }
            
            if (_damageService == null)
                Debug.LogError("Grenade: DamageService is null!");
            
            var origin = transform.position;
            ApplyAreaDamage(origin);
            
            var isOnline = Runner != null && Runner.IsRunning;
            
            if(isOnline)
                RPC_OnExploded(origin);
            else
                _signalBus.Fire(new GrenadeExplodedSignal(origin));
            
            if(isOnline && HasStateAuthority)
                DespawnTimer = TickTimer.CreateFromSeconds(Runner, DespawnDelaySeconds);
            else
                Destroy(gameObject);
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_OnExploded(Vector3 position)
        {
            Debug.Log("Rpc_OnExploded invoked");
            _signalBus?.Fire(new GrenadeExplodedSignal(position));
        }
        
        private void ApplyAreaDamage(Vector3 origin)
        {
            var hitCount = Physics.OverlapSphereNonAlloc(origin, _config.explosionRadius, _hitsBuffer, _config.damageableLayers);

            for (var i = 0; i < hitCount; i++)
            {
                var hit = _hitsBuffer[i];
                if (hit == null) continue;

                if (!hit.TryGetComponent<IDamageable>(out var damageable)) continue;

                var hitPoint  = hit.ClosestPoint(origin);
                var hitNormal = (hit.transform.position - origin).normalized;

                _damageService?.ApplyDamage(
                    _config.explosionDamage, damageable, hitPoint, hitNormal, gameObject);
            }
        }
    }
}