using System;
using _Project.Scripts.Core.Interfaces;
using _Project.Scripts.Core.Signals;
using _Project.Scripts.Presentation.Configs;
using _Project.Scripts.Services;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Presentation.Audio
{
    public class WeaponReloadAudioVisualizer : IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly IAudioService _audioService;
        private readonly WeaponAudioConfig _audioConfig;
        private readonly PlayerRegistry _playerRegistry;

        [Inject]
        public WeaponReloadAudioVisualizer(SignalBus signalBus, IAudioService audioService, WeaponAudioConfig config, PlayerRegistry playerRegistry)
        {
            _signalBus = signalBus;
            _audioService = audioService;
            _audioConfig = config;
            _playerRegistry = playerRegistry;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<ReloadStateChangedSignal>(OnReloadStateChanged);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<ReloadStateChangedSignal>(OnReloadStateChanged);
        }

        private void OnReloadStateChanged(ReloadStateChangedSignal signal)
        {
            Debug.Log($"OnReloadStateChanged: {signal.IsReloading}");
            if (!signal.IsReloading) return;

            var clip = _audioConfig.GetReloadSound(signal.WeaponName);
            if (clip == null) return;

            var playerObj = _playerRegistry.GetPlayer(signal.InputAuthority);
            var playerTransform = playerObj != null ? playerObj.transform : null;

            var fallbackPosition = playerTransform != null ? playerTransform.position : Vector3.zero;

            _audioService.Play3D(
                clip: clip,
                parent: playerTransform,
                position: fallbackPosition,
                volume: 0.75f,
                spatialBlend: 1f
            );
        }
    }
}