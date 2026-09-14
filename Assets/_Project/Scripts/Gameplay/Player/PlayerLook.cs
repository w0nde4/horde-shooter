using _Project.Scripts.Core.Data;
using _Project.Scripts.Domain.Gameplay.Configs;
using _Project.Scripts.Domain.Gameplay.Data;
using Fusion;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Player
{
    public class PlayerLook : NetworkBehaviour
    {
        private const float PixelsToDegreesScale = 0.022f;
        
        [Header("References")]
        [SerializeField] private Transform playerBody;
        [SerializeField] private Transform cameraTransform;

        [Inject] private PlayerConfig _playerConfig;

        [Networked] private float _networkedXRotation { get; set; }
        
        private float _xRotation;
        private float _yRotation;
        
        private PlayerIntent _intent;
        
        public PlayerIntent Intent
        {
            get => _intent;
            set => _intent = value;
        }

        public override void Spawned()
        {
            if (cameraTransform == null)
                cameraTransform = GetComponentInChildren<Camera>()?.transform;
            
            if (HasInputAuthority) return;
            
            var remoteCamera = GetComponentInChildren<Camera>();
            var remoteListener = GetComponentInChildren<AudioListener>();

            if (remoteCamera != null) remoteCamera.enabled = false;
            if (remoteListener != null) remoteListener.enabled = false;
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        
            if (playerBody == null)
                playerBody = transform;
        }
        
        public override void FixedUpdateNetwork()
        {
            if (GetInput(out NetworkInputData input))
            {
                ProcessLookData(input.Look);

                if (HasInputAuthority)
                {
                    _networkedXRotation = _xRotation;   
                }
                
                if (!HasInputAuthority && cameraTransform != null)
                {
                    cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
                }
            }
        }

        private void Update()
        {
            if (cameraTransform == null) return;

            if (Runner != null && Runner.IsRunning)
            {
                if (HasInputAuthority)
                {
                    var inputHandler = GetComponent<PlayerInputHandler>();
                    if (inputHandler != null)
                    {
                        ProcessLookData(inputHandler.RawMouseAccumulated);
                        cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
                        _networkedXRotation = _xRotation;
                    }
                }
                return; 
            }

            ApplyOfflineLook();
        }

        private void ProcessLookData(Vector2 rawAccumulatedMouse)
        {
            var finalSensitivity = _playerConfig.mouseSensitivity * PixelsToDegreesScale;
            
            _yRotation = rawAccumulatedMouse.x * finalSensitivity;
            
            var rawPitch = rawAccumulatedMouse.y * finalSensitivity;
            _xRotation = rawPitch * (_playerConfig.invertY ? 1f : -1f);
            
            _xRotation = Mathf.Clamp(_xRotation, _playerConfig.minPitch, _playerConfig.maxPitch);
            
            var inputHandler = GetComponent<PlayerInputHandler>();
            if (inputHandler != null)
            {
                var minRaw = _playerConfig.minPitch / (finalSensitivity * (_playerConfig.invertY ? 1f : -1f));
                var maxRaw = _playerConfig.maxPitch / (finalSensitivity * (_playerConfig.invertY ? 1f : -1f));
                
                if (minRaw > maxRaw) (minRaw, maxRaw) = (maxRaw, minRaw);

                inputHandler.ClampVerticalBuffer(minRaw, maxRaw);
            }

            if (playerBody != null)
            {
                playerBody.rotation = Quaternion.Euler(0, _yRotation, 0);
            }
        }
        
        private void ApplyOfflineLook()
        {
            var finalSensitivity = _playerConfig.mouseSensitivity * PixelsToDegreesScale;
            
            var mouseX = _intent.Look.x * finalSensitivity;
            var mouseY = _intent.Look.y * finalSensitivity * (_playerConfig.invertY ? 1f : -1f);

            playerBody.Rotate(Vector3.up * mouseX);

            _xRotation += mouseY;
            _xRotation = Mathf.Clamp(_xRotation, _playerConfig.minPitch, _playerConfig.maxPitch);

            cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        }
    }
}