using System;
using _Project.Scripts.Core.Interfaces;
using _Project.Scripts.Core.Signals;
using _Project.Scripts.Presentation.Configs;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Presentation.VFX
{
    public class GrenadeExplosionVFXVisualizer : IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly IVFXService _vfxService;
        private readonly GrenadeVFXConfig _config;

        public GrenadeExplosionVFXVisualizer(
            SignalBus signalBus,
            IVFXService vfxService,
            GrenadeVFXConfig config)
        {
            _signalBus = signalBus;
            _vfxService = vfxService;
            _config = config;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<GrenadeExplodedSignal>(OnExploded);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<GrenadeExplodedSignal>(OnExploded);
        }

        private void OnExploded(GrenadeExplodedSignal signal)
        {
            if (_config == null || _vfxService == null) return;
            if (_config.explosionPrefab == null) return;

            var instance = _vfxService.Spawn(
                _config.explosionPrefab,
                signal.Position,
                Quaternion.identity);

            AutoReleaseExplosion(instance, _config.lifetime).Forget();
        }

        private async UniTaskVoid AutoReleaseExplosion(GameObject instance, float delay)
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