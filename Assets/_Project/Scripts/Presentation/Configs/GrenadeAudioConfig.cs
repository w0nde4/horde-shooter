using UnityEngine;

namespace _Project.Scripts.Presentation.Configs
{
    [CreateAssetMenu(fileName = "GrenadeAudioConfig", menuName = "Presentation/Grenade Audio Config")]
    public class GrenadeAudioConfig : ScriptableObject
    {
        [Header("Explosion")]
        public AudioClip explosionClip;

        [Header("Mix")]
        [Range(0f, 1f)] public float explosionVolume = 1f;
        [Range(0f, 1f)] public float explosionSpatialBlend = 1f;
    }
}