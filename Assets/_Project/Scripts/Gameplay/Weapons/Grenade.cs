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
        [Inject] private IDamageService _damageService;
        [Inject] private SignalBus _signalBus;
        
        [Networked] private TickTimer _despawnTimer { get; set; }
        
        private GrenadeConfig _config;
        private bool _exploded;
        
        [Networked] private TickTimer _explosionTimer { get; set; } 

        public void Initialize(GrenadeConfig config)
        { 
            _config = config;
            
            if (Runner != null && Runner.IsRunning)
            {
                if (HasStateAuthority && !_explosionTimer.IsRunning)
                {
                    _explosionTimer = TickTimer.CreateFromSeconds(Runner, _config.explosionDelay);
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
            if (HasStateAuthority && !_exploded &&  _explosionTimer.Expired(Runner))
            {
                _explosionTimer = TickTimer.None;
                Explode();
            }
            
            if (HasStateAuthority && _exploded && _despawnTimer.Expired(Runner))
            {
                _despawnTimer = TickTimer.None;
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

            var hits = Physics.OverlapSphere(transform.position, _config.explosionRadius, _config.damageableLayers);
            foreach (var hit in hits)
            {
                var damageable = hit.GetComponent<IDamageable>();
                if (damageable == null) continue;
                
                var hitPoint = hit.ClosestPoint(transform.position);
                var hitNormal = (hit.transform.position - transform.position).normalized;
                if (_damageService == null) Debug.LogError("Grenade: DamageService is null!");
                _damageService?.ApplyDamage(_config.explosionDamage, damageable, hitPoint, hitNormal, gameObject);
            }

            if (Runner != null && Runner.IsRunning)
                Rpc_OnExploded(transform.position);
            else
                _signalBus?.Fire(new GrenadeExplodedSignal(transform.position));

            if (Runner != null && Runner.IsRunning && HasStateAuthority)
            {
                _despawnTimer = TickTimer.CreateFromSeconds(Runner, 0.2f);
            }
            else
            {
                Destroy(gameObject);
            }
            
            Debug.Log(Runner != null && Runner.IsRunning ? "Server Boom!" : "Offline Boom!");
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void Rpc_OnExploded(Vector3 position)
        {
            Debug.Log("Rpc_OnExploded invoked");
            _signalBus?.Fire(new GrenadeExplodedSignal(position));
        }
    }
}