using UnityEngine;

namespace _Project.Scripts.Domain.Gameplay.Configs
{
    [CreateAssetMenu(fileName = "WaveCycleConfig", menuName = "Waves/Wave Cycle Config")]
    public class WaveCycleConfig : ScriptableObject
    {
        [Header("Timings")]
        public float intermissionDuration = 5f;
        public float countdownDuration = 3f;

        [Header("Waves")]
        public WaveConfig[] waves;
    }
}