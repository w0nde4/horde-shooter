using System;
using _Project.Scripts.Core.Data;
using _Project.Scripts.Core.Interfaces;
using _Project.Scripts.Core.Signals;
using _Project.Scripts.Domain.Gameplay;
using _Project.Scripts.Domain.Gameplay.Configs;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;
using Random = UnityEngine.Random;

namespace _Project.Scripts.Gameplay.Weapons
{
    public abstract class WeaponBase : MonoBehaviour
    {
        [SerializeField] protected WeaponConfig config;
        
        [Inject] private PlayerStats _playerStats;
        [Inject] private SignalBus _signalBus;
        [Inject] private IDamageService _damageService;
        [Inject] private IShootingService _shootingService;
    
        public bool IsReloading => _isReloading;
        public int CurrentAmmo => _currentAmmo;
        public int MagazineSize => config != null ? config.magazineSize : 0;
        public string WeaponName => config != null ? config.weaponName : "Unnamed";
        public bool IsAutomatic => config != null && config.isAutomatic;

        private int _currentAmmo;
        private bool _isReloading;
        private float _nextFireTime;
        private Transform _shootOrigin;
    
        public virtual void Initialize(Transform origin)
        {
            _shootOrigin = origin;
            _currentAmmo = config.magazineSize;
            _isReloading = false;
            _nextFireTime = 0f;
        }

        public virtual bool CanFire()
        {
            if (config == null || _isReloading || _currentAmmo <= 0)
                return false;
            return Time.time >= _nextFireTime;
        }

        public virtual void TryFire()
        {
            if (!CanFire()) return;

            Fire();
            _currentAmmo--;
            _nextFireTime = Time.time + 1f / config.fireRate;

            if (_currentAmmo <= 0)
                Reload();
        }

        protected abstract void Fire();

        public virtual void Reload()
        {
            if (_isReloading || _currentAmmo == config.magazineSize)
                return;

            ReloadAsync().Forget();
        }

        protected virtual async UniTask ReloadAsync()
        {
            _isReloading = true;
            Debug.Log($"{WeaponName}: Reloading...");

            try
            {
                await UniTask.Delay(
                    TimeSpan.FromSeconds(config.reloadTime),
                    cancellationToken: this.GetCancellationTokenOnDestroy()
                );

                _currentAmmo = config.magazineSize;
                Debug.Log($"{WeaponName}: Reload complete.");
            }
            finally
            {
                _isReloading = false;
            }
        }

        protected ShotResult PerformRaycast()
        {
            var shot = _shootingService.CalculateShotResult(
                _shootOrigin.position, 
                _shootOrigin.forward, 
                config.range, 
                config.spreadAngle,
                config.hitLayers
                );
            
            if(shot.Hit && shot.HitCollider != null)
            {
                var damageable = shot.HitCollider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    var finalDamage = config.damage * (_playerStats?.DamageMultiplier ?? 1f);
                    _damageService?.ApplyDamage(finalDamage, damageable, shot.HitPoint, shot.HitNormal, gameObject);
                }
            }
            _signalBus.Fire(new ShotFiredSignal(shot));
            return shot;
        }
    }
}