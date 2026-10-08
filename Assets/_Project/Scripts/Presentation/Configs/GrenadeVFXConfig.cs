using UnityEngine;

namespace _Project.Scripts.Presentation.Configs
{
    [CreateAssetMenu(fileName = "GrenadeVFXConfig", menuName = "Presentation/Grenade VFX Config")]
    public class GrenadeVFXConfig : ScriptableObject
    {
        [Header("Explosion")]
        public GameObject explosionPrefab;
        
        [Header("Timing")]
        [Tooltip("Return the instance to the pool after a few seconds." +
                 " The duration must be greater than or equal to the duration of the VFX itself.")]
        public float lifetime = 2f;
    }
}