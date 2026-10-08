using System;
using _Project.Scripts.Core.Interfaces;
using _Project.Scripts.Core.Signals;
using _Project.Scripts.Presentation.Configs;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Presentation.Audio
{
    public class GrenadeExplosionAudioVisualizer : IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly IAudioService _audioService;
        private readonly GrenadeAudioConfig _audioConfig;

        [Inject]
        public GrenadeExplosionAudioVisualizer(
            SignalBus signalBus,
            IAudioService audioService,
            GrenadeAudioConfig audioConfig)
        {
            _signalBus = signalBus;
            _audioService = audioService;
            _audioConfig = audioConfig;
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
            Debug.Log("Audio config: " + _audioConfig + "Audio service: " + _audioService);
            if (_audioConfig == null || _audioService == null) return;

            var clip = _audioConfig.explosionClip;
            if (clip == null)
            {
                Debug.LogWarning("[GrenadeExplosionAudioVisualizer] Explosion clip is not assigned!");
                return;
            }

            _audioService.Play3D(
                clip: clip,
                parent: null,
                position: signal.Position,
                volume: _audioConfig.explosionVolume,
                spatialBlend: _audioConfig.explosionSpatialBlend
            );
        }
    }
}