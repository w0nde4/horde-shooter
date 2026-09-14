using _Project.Scripts.Domain.Gameplay;
using _Project.Scripts.Domain.Gameplay.Configs;
using _Project.Scripts.Domain.Gameplay.Data;
using Fusion;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Gameplay.Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInputHandler))]
    public class PlayerController : NetworkBehaviour
    {
        [Inject] private PlayerConfig _config;
        [Inject] private PlayerStats _stats;
    
        private NetworkCharacterController _controller;
        private PlayerInputHandler _inputHandler;
        private float _verticalVelocity;
        
        [Networked] private NetworkButtons _oldButtons { get; set; }

        private void Awake()
        {
            _controller = GetComponent<NetworkCharacterController>();
            _inputHandler = GetComponent<PlayerInputHandler>();
        }

        public override void FixedUpdateNetwork()
        {
            if (GetInput(out NetworkInputData input))
            {
                var pressed = input.Buttons.GetPressed(_oldButtons);
                ApplyNetworkMovement(input, pressed);
                _oldButtons = input.Buttons;
            }
        }

        private void Update()
        {
            if (Runner == null || !Runner.IsRunning)
            {
                ApplyOfflineMovement();
            }
        }

        private void ApplyNetworkMovement(NetworkInputData input, NetworkButtons pressed)
        {
            var baseSpeed = _stats?.MoveSpeed ?? _config.defaultMoveSpeed;
            var isSprinting = input.Buttons.IsSet(PlayerButtons.Sprint);
            var currentSpeed = isSprinting ? baseSpeed * _config.sprintMultiplier : baseSpeed;

            _controller.maxSpeed = currentSpeed;
            var moveDirection = transform.right * input.Move.x + transform.forward * input.Move.y;

            if (pressed.IsSet(PlayerButtons.Jump) && _controller.Grounded)
            {
                _controller.Jump();
            }
                
            _controller.Move(moveDirection);
        }

        private void ApplyOfflineMovement()
        {
            var intent = _inputHandler.LocalOfflineIntent;
            var baseSpeed = _stats?.MoveSpeed ?? _config.defaultMoveSpeed; 
            var currentSpeed = intent.Sprint ? baseSpeed * _config.sprintMultiplier : baseSpeed;

            _controller.maxSpeed = currentSpeed;
            var moveDirection = transform.right * intent.Move.x + transform.forward * intent.Move.y;
        
            if (intent.Jump && _controller.Grounded)
            {
                _controller.Jump();
            }
                
            _controller.Move(moveDirection);
        }
    }
}