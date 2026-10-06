using _Project.Scripts.Domain.Gameplay;
using _Project.Scripts.Domain.Gameplay.Configs;
using _Project.Scripts.Domain.Gameplay.Data;
using _Project.Scripts.Services;
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
        [Inject] private PlayerRegistry _playerRegistry;
    
        private NetworkCharacterController _controller;
        
        [Networked] private NetworkButtons _oldButtons { get; set; }

        private void Awake()
        {
            _controller = GetComponent<NetworkCharacterController>();
        }

        public override void Spawned()
        {
            _playerRegistry.RegisterPlayer(gameObject, Object.InputAuthority.ToString());
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            _playerRegistry.UnregisterPlayer(Object.InputAuthority.ToString());
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
    }
}