using UnityEngine;

namespace _Project.Scripts.Core.Interfaces
{
    public interface IAudioService
    {
        void Play2D(AudioClip clip, float volume = 1f);
        void Play3D(AudioClip clip, Transform parent, Vector3 position, float volume = 1f, float spatialBlend = 1f);
    }
}