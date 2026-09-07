using UnityEngine;

namespace _Project.Scripts.Domain.Gameplay.Configs
{
    [CreateAssetMenu(fileName = "New Grenade Config", menuName = "Weapons/Grenade Config")]
    public class GrenadeConfig : ScriptableObject
    {
        [Header("Explosion")]
        public float explosionDelay = 3f;
        public float explosionRadius = 5f;
        public float explosionDamage = 50f;
        public LayerMask damageableLayers = ~0;

        [Header("Throw")]
        public float throwForce = 15f;
        public float upwardForce = 5f;
        public float throwCooldown = 5f;
    
        [Header("Prefab Address")]
        public string prefabAddress;
    }
}