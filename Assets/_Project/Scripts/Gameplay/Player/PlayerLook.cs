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

        [Networked] private float NetworkedXRotation { get; set; }
        
        private float _xRotation;
        private float _yRotation;
        private PlayerIntent _intent;
        private PlayerInputHandler _inputHandler;
        
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

        private void Awake()
        {
            _inputHandler = GetComponent<PlayerInputHandler>();
            
            if (playerBody == null)
                playerBody = transform;            
        }
        
        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;   
        }
        
        public override void FixedUpdateNetwork()
        {
            if (GetInput(out NetworkInputData input))
            {
                ProcessLookData(input.Look);

                if (HasInputAuthority)
                {
                    NetworkedXRotation = _xRotation;   
                }
            }
            if (!HasInputAuthority && cameraTransform != null)
            {
                cameraTransform.localRotation = Quaternion.Euler(NetworkedXRotation, 0f, 0f);
            }
        }

        private void Update()
        {
            if (cameraTransform == null) return;
            if(!HasInputAuthority) return;

            if (_inputHandler != null)
            {
                ProcessLookData(_inputHandler.RawMouseAccumulated);
                cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
                NetworkedXRotation = _xRotation;
            }
        }

        private void ProcessLookData(Vector2 rawAccumulatedMouse)
        {
            var finalSensitivity = _playerConfig.mouseSensitivity * PixelsToDegreesScale;
            
            _yRotation = rawAccumulatedMouse.x * finalSensitivity;
            
            var rawPitch = rawAccumulatedMouse.y * finalSensitivity;
            _xRotation = rawPitch * (_playerConfig.invertY ? 1f : -1f);
            
            _xRotation = Mathf.Clamp(_xRotation, _playerConfig.minPitch, _playerConfig.maxPitch);
            
            if (_inputHandler != null)
            {
                var minRaw = _playerConfig.minPitch / (finalSensitivity * (_playerConfig.invertY ? 1f : -1f));
                var maxRaw = _playerConfig.maxPitch / (finalSensitivity * (_playerConfig.invertY ? 1f : -1f));
                
                if (minRaw > maxRaw) (minRaw, maxRaw) = (maxRaw, minRaw);

                _inputHandler.ClampVerticalBuffer(minRaw, maxRaw);
            }

            if (playerBody != null)
            {
                playerBody.rotation = Quaternion.Euler(0, _yRotation, 0);
            }
        }
    }
}