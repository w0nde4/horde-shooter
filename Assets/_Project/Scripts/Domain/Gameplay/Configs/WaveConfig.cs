using UnityEngine;

namespace _Project.Scripts.Domain.Gameplay.Configs
{
    [CreateAssetMenu(fileName = "New Wave Config", menuName = "Waves/Wave Config")]
    public class WaveConfig : ScriptableObject
    {
        [Header("Wave Settings")]
        public int waveNumber = 1;
        public int enemyCount = 5;
        public float spawnInterval = 1f;
        public float enemySpeed = 3.5f;
    }
}