using _Project.Scripts.Core.Data;
using _Project.Scripts.Core.Interfaces;
using _Project.Scripts.Core.Signals;
using _Project.Scripts.Domain.Gameplay;
using _Project.Scripts.Domain.Gameplay.Configs;
using _Project.Scripts.Domain.Gameplay.Data;
using Fusion;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Weapons
{
    public abstract class WeaponBase : NetworkBehaviour
    {
        private const int BulletBufferCapacity = 32;
        
        [SerializeField] protected WeaponConfig config;
        
        [Inject] private PlayerStats _playerStats;
        [Inject] private SignalBus _signalBus;
        [Inject] private IDamageService _damageService;
        [Inject] private IShootingService _shootingService;

        [Networked] private int NetworkedAmmo { get; set; } 
        [Networked] private TickTimer ReloadTimer { get; set; }
        [Networked] private TickTimer FireDelayTimer { get; set; }

        [Networked, Capacity(BulletBufferCapacity)]
        private NetworkArray<NetworkBulletData> BulletBuffer => default;
        
        [Networked, OnChangedRender(nameof(OnBulletCounterChanged))]
        private int TotalBulletsFiredCounter { get; set; }
    
        private int _lastRenderProcessBullets;
        private Transform _shootOrigin;
        
        public bool IsReloading => !ReloadTimer.ExpiredOrNotRunning(Runner);
        public int CurrentAmmo => NetworkedAmmo;
        public int MagazineSize => config != null ? config.magazineSize : 0;
        public string WeaponName => config != null ? config.weaponName : "Unnamed";
        public bool IsAutomatic => config != null && config.isAutomatic;

    
        public virtual void Initialize(Transform origin)
        {
            _shootOrigin = origin;
            NetworkedAmmo = config.magazineSize;
        }

        public override void Spawned()
        {
            _lastRenderProcessBullets = TotalBulletsFiredCounter;

            if (HasStateAuthority && NetworkedAmmo == 0 && config != null)
            {
                NetworkedAmmo = config.magazineSize;
            }
        }

        public virtual bool CanFire()
        {
            if (config == null || IsReloading || NetworkedAmmo <= 0)
                return false;

            return FireDelayTimer.ExpiredOrNotRunning(Runner);
        }

        public virtual void TryFire()
        {
            if (!CanFire()) return;

            Fire();
            
            NetworkedAmmo--;
            FireDelayTimer = TickTimer.CreateFromSeconds(Runner, 1f / config.fireRate);
            
            if (NetworkedAmmo <= 0)
                Reload();
        }

        protected abstract void Fire();

        public virtual void Reload()
        {
            if (IsReloading || NetworkedAmmo == config.magazineSize)
                return;

            Debug.Log($"{WeaponName}: Reloading via network");
            ReloadTimer = TickTimer.CreateFromSeconds(Runner, config.reloadTime);
        }

        public override void FixedUpdateNetwork()
        {
            if(!HasStateAuthority) return;

            if (ReloadTimer.Expired(Runner))
            {
                NetworkedAmmo = config.magazineSize;
                ReloadTimer = TickTimer.None;
                Debug.Log($"{WeaponName}: Reload complete via network");
            }
        }

        //todo: delete after network implementation
        // protected virtual async UniTask ReloadAsync()
        // {
        //     _isReloading = true;
        //     Debug.Log($"{WeaponName}: Reloading...");
        //
        //     try
        //     {
        //         await UniTask.Delay(
        //             TimeSpan.FromSeconds(config.reloadTime),
        //             cancellationToken: this.GetCancellationTokenOnDestroy()
        //         );
        //
        //         _currentAmmo = config.magazineSize;
        //         Debug.Log($"{WeaponName}: Reload complete.");
        //     }
        //     finally
        //     {
        //         _isReloading = false;
        //     }
        // }

        protected ShotResult PerformRaycast()
        {
            var shot = _shootingService.CalculateShotResult(
                _shootOrigin.position, 
                _shootOrigin.forward, 
                config.range, 
                config.spreadAngle,
                config.hitLayers
                );
            
            if(HasStateAuthority && shot.Hit && shot.HitCollider != null)
            {
                var damageable = shot.HitCollider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    var finalDamage = config.damage * (_playerStats?.DamageMultiplier ?? 1f);
                    _damageService?.ApplyDamage(finalDamage, damageable, shot.HitPoint, shot.HitNormal, gameObject);
                }
            }
            
            var bufferIndex = TotalBulletsFiredCounter % BulletBufferCapacity;

            BulletBuffer.Set(bufferIndex, new NetworkBulletData
            {
                Hit = shot.Hit,
                HitPoint = shot.HitPoint
            });
            
            TotalBulletsFiredCounter++;

            if (Runner.IsForward)
            {
                TriggerLocalShot(shot.Hit, shot.HitPoint);
                _lastRenderProcessBullets = TotalBulletsFiredCounter;
            }
            
            return shot;
        }

        private void OnBulletCounterChanged()
        {
            if (TotalBulletsFiredCounter > _lastRenderProcessBullets)
            {
                var bulletsToProcess = TotalBulletsFiredCounter - _lastRenderProcessBullets;

                for (var i = 0; i < bulletsToProcess; i++)
                {
                    var targetIndex = (_lastRenderProcessBullets + i) % BulletBufferCapacity;
                    var data = BulletBuffer[targetIndex];

                    TriggerLocalShot(data.Hit, data.HitPoint);
                }

                _lastRenderProcessBullets = TotalBulletsFiredCounter;
            }
        }

        private void TriggerLocalShot(bool isHit, Vector3 hitPoint)
        {
            var visualShot = new ShotResult
            {
                Hit = isHit,
                HitPoint = hitPoint
            };

            _signalBus.Fire(new ShotFiredSignal(visualShot));
        }
    }
}