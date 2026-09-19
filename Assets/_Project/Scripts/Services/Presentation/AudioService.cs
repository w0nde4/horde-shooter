using System;
using _Project.Scripts.Core.Data;
using _Project.Scripts.Core.Interfaces;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace _Project.Scripts.Services.Presentation
{
    public class AudioService : IAudioService
    {
        private readonly ObjectPool<AudioSource> _sourcePool;
        private readonly Transform _audioRoot;

        public AudioService()
        {
            _audioRoot = new GameObject("[AUDIO_POOL]").transform;
            Object.DontDestroyOnLoad(_audioRoot.gameObject);

            _sourcePool = new ObjectPool<AudioSource>(
                createFunc: () =>
                {
                    var go = new GameObject("PooledAudioSource");
                    go.transform.SetParent(_audioRoot);
                    var source = go.AddComponent<AudioSource>();
                    source.playOnAwake = false;
                    return source;
                },
                actionOnGet: (source) => source.gameObject.SetActive(true),
                actionOnRelease: (source) => {
                    source.Stop();
                    source.clip = null;
                    source.gameObject.SetActive(false);
                },
                actionOnDestroy: (source) => Object.Destroy(source.gameObject),
                collectionCheck: false,
                defaultCapacity: 10,
                maxSize: 40
            );
        }
        
        public void Play2D(AudioClip clip, float volume = 1)
        {
            if (clip == null) return;
            PlayInternal(clip, Vector3.zero, volume, 0f); //0 = 2D
        }

        public void Play3D(AudioClip clip, Transform parent, Vector3 position, float volume = 1f, float spatialBlend = 1f)
        {
            if (clip == null) return;
            PlayInternal(clip, position, volume, spatialBlend, parent); //1 = full 3D
        }

        private void PlayInternal(AudioClip clip, Vector3 position, float volume, float spatialBlend, Transform parent = null)
        {
            var source = _sourcePool.Get();
            
            if (parent == null)
                source.transform.position = position;
            
            source.clip = clip;
            source.volume = volume;
            source.spatialBlend = spatialBlend;

            if (parent != null)
            {
                source.transform.SetParent(parent);
                source.transform.localPosition = Vector3.zero;
            }

            source.Play();

            ReturnToPoolAfterPlayback(source, clip.length, _audioRoot).Forget();
        }

        private async UniTaskVoid ReturnToPoolAfterPlayback(AudioSource source, float duration, Transform poolRoot)
        {
            if (source == null) return;

            await UniTask.Delay(TimeSpan.FromSeconds(duration));

            if (source != null)
            {
                source.transform.SetParent(poolRoot); 
                _sourcePool.Release(source);
            }
        }
    }
}