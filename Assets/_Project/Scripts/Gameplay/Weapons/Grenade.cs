using _Project.Scripts.Core.Interfaces;
using _Project.Scripts.Domain.Gameplay.Configs;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Weapons
{
    public class Grenade : MonoBehaviour
    {
        [Inject] private IDamageService _damageService;
        
        private GrenadeConfig _config;
        private bool _exploded;

        public void Initialize(GrenadeConfig config)
        {
            this._config = config;
            ExplosionTimerAsync().Forget();
        }

        private async UniTask ExplosionTimerAsync()
        {
            await UniTask.Delay(
                System.TimeSpan.FromSeconds(_config.explosionDelay),
                cancellationToken: this.GetCancellationTokenOnDestroy()
            );

            Explode();
        }

        private void Explode()
        {
            if (_exploded) return;
            _exploded = true;

            var hits = Physics.OverlapSphere(transform.position, _config.explosionRadius, _config.damageableLayers);
            foreach (var hit in hits)
            {
                var damageable = hit.GetComponent<IDamageable>();
                if (damageable == null) continue;
                
                var hitPoint = hit.ClosestPoint(transform.position);
                var hitNormal = (hit.transform.position - transform.position).normalized;
                                
                _damageService?.ApplyDamage(_config.explosionDamage, damageable, hitPoint, hitNormal, gameObject);
                
                //knockback effect (works only for rigidbodies)
                //going to add force for enemies and player (non rigidbodies)
                // var rb = hit.GetComponent<Rigidbody>();
                // if (rb != null)
                //     rb.AddExplosionForce(500f, transform.position, config.explosionRadius, 1f, ForceMode.Impulse);
            }

            Debug.Log("Boom!");
            Destroy(gameObject);
        }
    }
}