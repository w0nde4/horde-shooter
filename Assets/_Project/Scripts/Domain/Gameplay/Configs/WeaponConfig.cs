using UnityEngine;

namespace _Project.Scripts.Domain.Gameplay.Configs
{
    [CreateAssetMenu(fileName = "New Weapon Config", menuName = "Weapons/Weapon Config")]
    public class WeaponConfig : ScriptableObject
    {
        [Header("General")]
        public string weaponName = "Weapon";
        public float damage = 10f;
        public float range = 100f;

        [Header("Fire Rate")]
        [Tooltip("per sec")]
        public float fireRate = 10f;

        [Header("Magazine")]
        public int magazineSize = 12;
        public float reloadTime = 1.5f;

        [Header("Spread")]
        [Tooltip("degrees (0 = precise shot)")]
        public float spreadAngle = 0.5f;
    
        [Header("Pellets (shotgun only)")]
        [Tooltip("for single shot weapon = 1")]
        public int pellets = 1;

        [Header("Automatic")]
        [Tooltip("true = fire by holding")]
        public bool isAutomatic = false;
        
        [Header("Hit Layer")]
        public LayerMask hitLayers = ~0;
    }
}