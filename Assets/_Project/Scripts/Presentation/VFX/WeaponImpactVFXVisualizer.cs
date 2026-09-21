using System;
using _Project.Scripts.Core.Interfaces;
using _Project.Scripts.Domain.Gameplay.Signals;
using _Project.Scripts.Presentation.Configs;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Presentation.VFX
{
    public class WeaponImpactVFXVisualizer : IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly IVFXService _vfxService;
        private readonly WeaponVFXConfig _vfxConfig;

        private int _environmentLayer;
        private int _enemyLayer;

        public WeaponImpactVFXVisualizer(SignalBus signalBus, IVFXService vfxService, WeaponVFXConfig vfxConfig)
        {
            _signalBus = signalBus;
            _vfxService = vfxService;
            _vfxConfig = vfxConfig;
        }

        public void Initialize()
        {
            _environmentLayer = LayerMask.NameToLayer("Environment");
            _enemyLayer = LayerMask.NameToLayer("Enemy");

            _signalBus.Subscribe<ShotFiredSignal>(OnShotFired);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<ShotFiredSignal>(OnShotFired);
        }

        private void OnShotFired(ShotFiredSignal signal)
        {
            if (!signal.Shot.Hit) return;
            if (_vfxConfig == null || _vfxService == null) return;

            if (!_vfxConfig.TryGetMapping(signal.WeaponName, out var vfx)) return;

            var hitLayer = _environmentLayer;
            
            if(signal.Shot.HitCollider != null) 
                hitLayer = signal.Shot.HitCollider.gameObject.layer;
            
            GameObject prefabToSpawn = null;
            
            if (hitLayer == _environmentLayer)
            {
                prefabToSpawn = vfx.defaultImpactPrefab;
            }
            else if (hitLayer == _enemyLayer)
            {
                prefabToSpawn = vfx.fleshImpactPrefab;
            }
            else
            {
                prefabToSpawn = vfx.defaultImpactPrefab;
            }

            if (prefabToSpawn == null) return;

            var impactRotation = signal.Shot.HitNormal != Vector3.zero 
                ? Quaternion.LookRotation(signal.Shot.HitNormal) 
                : Quaternion.identity;

            var impactInstance = _vfxService.Spawn(prefabToSpawn, signal.Shot.HitPoint, impactRotation);
            
            AutoReleaseImpact(impactInstance, 1.5f).Forget();
        }

        private async UniTaskVoid AutoReleaseImpact(GameObject instance, float delay)
        {
            if (instance == null) return;
            await UniTask.Delay(TimeSpan.FromSeconds(delay));
            
            if (instance != null && instance.activeSelf)
            {
                _vfxService.Release(instance);
            }
        }
    }
}