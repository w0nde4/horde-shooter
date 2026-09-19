using System;
using _Project.Scripts.Core.Interfaces;
using _Project.Scripts.Domain.Gameplay.Signals;
using _Project.Scripts.Presentation.Configs;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Presentation.VFX
{
    public class WeaponTracerVFXVisualizer : IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly IVFXService _vfxService;
        private readonly WeaponVFXConfig _vfxConfig;

        public WeaponTracerVFXVisualizer(SignalBus signalBus, IVFXService vfxService, WeaponVFXConfig vfxConfig)
        {
            _signalBus = signalBus;
            _vfxService = vfxService;
            _vfxConfig = vfxConfig;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<ShotFiredSignal>(OnShotFired);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<ShotFiredSignal>(OnShotFired);
        }

        private void OnShotFired(ShotFiredSignal signal)
        {
            if (_vfxConfig == null || _vfxService == null) return;

            if (!_vfxConfig.TryGetMapping(signal.WeaponName, out var vfx) || vfx.tracerPrefab == null) return;

            Vector3 targetPosition;
            if (signal.Shot.Hit)
            {
                targetPosition = signal.Shot.HitPoint;
            }
            else
            {
                targetPosition = signal.MuzzlePosition + (signal.MuzzleRotation * Vector3.forward * 50f);
            }

            var tracerInstance = _vfxService.Spawn(vfx.tracerPrefab, signal.MuzzlePosition, Quaternion.identity);
            if (tracerInstance == null) return;

            if (tracerInstance.TryGetComponent<TrailRenderer>(out var trail))
            {
                trail.Clear();
            }

            var distance = Vector3.Distance(signal.MuzzlePosition, targetPosition);
            var flyDuration = distance / vfx.tracerSpeed;

            tracerInstance.transform.DOMove(targetPosition, flyDuration)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    AutoReleaseTracer(tracerInstance, trail != null ? trail.time : 0.15f).Forget();
                });
        }

        private async UniTaskVoid AutoReleaseTracer(GameObject instance, float delay)
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