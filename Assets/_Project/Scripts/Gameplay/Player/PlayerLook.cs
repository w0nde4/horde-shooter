using _Project.Scripts.Core.Data;
using _Project.Scripts.Domain.Gameplay.Configs;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace _Project.Scripts.Gameplay.Player
{
    public class PlayerLook : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform playerBody;
        [SerializeField] private Transform cameraTransform;

        [Inject] private PlayerConfig _playerConfig;

        private float _xRotation;
        private PlayerIntent _intent;
        
        public PlayerIntent Intent
        {
            get => _intent;
            set => _intent = value; 
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        
            if (playerBody == null)
                playerBody = transform;

            if (cameraTransform != null) return;
            var cam = GetComponentInChildren<Camera>();
            if (cam != null)
                cameraTransform = cam.transform;
            else
                Debug.LogError("PlayerLook: Camera not found! Assign cameraTransform manually.");
        }

        private void Update()
        {
            if (cameraTransform == null) return;

            var mouseX = _intent.Look.x * _playerConfig.mouseSensitivity * Time.deltaTime;
            var mouseY = _intent.Look.y * _playerConfig.mouseSensitivity * Time.deltaTime * (_playerConfig.invertY ? 1f : -1f);

            playerBody.Rotate(Vector3.up * mouseX);

            _xRotation += mouseY;
            _xRotation = Mathf.Clamp(_xRotation, _playerConfig.minPitch, _playerConfig.maxPitch);

            cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        }
    
        //todo: delete it after bridge done
        // public void OnLook(InputValue value) 
        //     => _lookInput = value.Get<Vector2>();
    }
}