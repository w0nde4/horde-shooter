using _Project.Scripts.Core.Data;
using _Project.Scripts.Core.Interfaces;
using _Project.Scripts.Core.Signals;
using _Project.Scripts.Domain.Gameplay;
using _Project.Scripts.Domain.Gameplay.Configs;
using _Project.Scripts.Domain.Gameplay.Data;
using _Project.Scripts.Domain.Gameplay.Signals;
using Fusion;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Weapons
{
    public abstract class WeaponBase : NetworkBehaviour
    {
        private const int BulletBufferCapacity = 32;
        
        [SerializeField] protected WeaponConfig config;
        [Header("Marker for VFX")]
        [SerializeField] private Transform muzzleMarker;
        
        [Inject] private PlayerStats _playerStats;
        [Inject] private SignalBus _signalBus;
        [Inject] private IDamageService _damageService;
        [Inject] private IShootingService _shootingService;

        [Networked] private int NetworkedAmmo { get; set; }
        [Networked] private TickTimer FireDelayTimer { get; set; }
        [Networked] private TickTimer ReloadTimer { get; set; }

        [Networked, Capacity(BulletBufferCapacity)]
        private NetworkArray<NetworkBulletData> BulletBuffer => default;

        [Networked, OnChangedRender(nameof(OnBulletCounterChanged))]
        private int TotalBulletsFiredCounter { get; set; }
        
        [Networked, OnChangedRender(nameof(OnReloadStateChanged))]
        private NetworkBool IsNetworkReloading { get; set; }

        private int _lastRenderProcessBullets;
        private Transform _shootOrigin;
        
        public bool IsReloading => !ReloadTimer.ExpiredOrNotRunning(Runner);
        public int CurrentAmmo => NetworkedAmmo;
        public int MagazineSize => config != null ? config.magazineSize : 0;
        public string WeaponName => config != null ? config.weaponName : "Unnamed";
        public bool IsAutomatic => config != null && config.isAutomatic;
        public Transform MuzzleTransform => muzzleMarker != null ? muzzleMarker : transform;

    
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

            IsNetworkReloading = true;
        }

        public override void FixedUpdateNetwork()
        {
            if(!HasStateAuthority) return;

            if (ReloadTimer.Expired(Runner))
            {
                NetworkedAmmo = config.magazineSize;
                ReloadTimer = TickTimer.None;
                IsNetworkReloading = false;
                Debug.Log($"{WeaponName}: Reload complete via network");
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
                TriggerLocalShot(shot);
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

                    var proxyShot = new ShotResult
                    {
                        Hit = data.Hit,
                        HitPoint = data.HitPoint,
                        HitNormal = Vector3.up,
                        HitCollider = null
                    };
                    
                    TriggerLocalShot(proxyShot);
                }

                _lastRenderProcessBullets = TotalBulletsFiredCounter;
            }
        }

        private void OnReloadStateChanged()
        {
            var isCurrentlyReloading = IsNetworkReloading;
            _signalBus.Fire(new ReloadStateChangedSignal(
                WeaponName, 
                Object.InputAuthority, 
                isCurrentlyReloading
                )
            );
            Debug.Log($"[Fusion-Callback] Triggered for {WeaponName}, State: {isCurrentlyReloading}");
        }

        private void TriggerLocalShot(ShotResult shotResult)
        {
            var muzzle = MuzzleTransform;
            
            _signalBus.Fire(new ShotFiredSignal(
                shotResult,
                WeaponName,
                Object.InputAuthority,
                muzzle.position,
                muzzle.rotation
                )
            );
        }
    }
}