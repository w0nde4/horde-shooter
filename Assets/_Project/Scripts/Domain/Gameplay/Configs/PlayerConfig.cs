using UnityEngine;

namespace _Project.Scripts.Domain.Gameplay.Configs
{
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "Player/Player Config")]
    public class PlayerConfig : ScriptableObject
    {
        [Header("Movement")]
        public float defaultMoveSpeed = 5f;
        public float sprintMultiplier = 2f;

        [Header("Look")]
        public float mouseSensitivity = 100f;
        public float minPitch = -80f;
        public float maxPitch = 80f;
        public bool invertY = false;
        
        [Header("Addressables")]
        public string playerPrefabAddress;
    }
}