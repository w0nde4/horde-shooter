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
        [Tooltip("Выстрелов в секунду")]
        public float fireRate = 10f;

        [Header("Magazine")]
        public int magazineSize = 12;
        public float reloadTime = 1.5f;

        [Header("Spread")]
        [Tooltip("Максимальный угол разброса в градусах (0 = точный выстрел)")]
        public float spreadAngle = 0.5f;
    
        [Header("Pellets (для дробовика)")]
        [Tooltip("Количество дробинок за выстрел. Для одиночного оружия = 1")]
        public int pellets = 1;

        [Header("Automatic")]
        public bool isAutomatic = false; // если true, оружие стреляет при удержании кнопки
        
        [Header("Hit Layer")]
        public LayerMask hitLayers = ~0; // по умолчанию все слои
    }
}