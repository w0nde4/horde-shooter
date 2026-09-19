using System;
using _Project.Scripts.Core.Interfaces;
using _Project.Scripts.Domain.Gameplay.Signals;
using _Project.Scripts.Presentation.Configs;
using _Project.Scripts.Services;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Presentation.Visuals
{
    public class WeaponFireAudioVisualizer : IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly IAudioService _audioService;
        private readonly WeaponAudioConfig _audioConfig;
        private readonly PlayerRegistry _players;

        [Inject]
        public WeaponFireAudioVisualizer(SignalBus signalBus, IAudioService audioService, WeaponAudioConfig config, PlayerRegistry players)
        {
            _signalBus = signalBus;
            _audioService = audioService;
            _audioConfig = config;
            _players = players;
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
            var clip = _audioConfig.GetShotSound(signal.WeaponName);
            
            if (clip == null)
            {
                Debug.LogWarning($"[AudioVisualizer] Audio sound for '{signal.WeaponName}' does not found!");
                return;
            }

            var playerObj = _players.GetPlayer(signal.InputAuthority);
            var playerTransform = playerObj != null ? playerObj.transform : null;

            _audioService.Play3D(
                clip: clip,
                parent: playerTransform,
                position: signal.MuzzlePosition, 
                volume: 0.85f, 
                spatialBlend: 1f
            );
        }
    }
}