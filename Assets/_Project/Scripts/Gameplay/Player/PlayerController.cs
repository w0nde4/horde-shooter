using _Project.Scripts.Core.Data;
using _Project.Scripts.Domain.Gameplay;
using _Project.Scripts.Domain.Gameplay.Configs;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace _Project.Scripts.Gameplay.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Inject] private PlayerConfig _config;
        [Inject] private PlayerStats _stats;
    
        private CharacterController _controller;
        private PlayerIntent _intent;
        private float _verticalVelocity;
        
        public PlayerIntent CurrentIntent => _intent;

        private void Awake() => _controller = GetComponent<CharacterController>();

        private void Update()
        {
            ApplyMovement();
        }

        private void ApplyMovement()
        {
            var baseSpeed = _stats?.MoveSpeed ?? _config.defaultMoveSpeed; 
            var currentSpeed = _intent.Sprint ? baseSpeed * _config.sprintMultiplier : baseSpeed;
            
            var moveDirection = transform.right * _intent.Move.x 
                + transform.forward * _intent.Move.y;

            if (_controller.isGrounded)
            {
                if(_verticalVelocity < 0) _verticalVelocity = -2f;

                if (_intent.Jump)
                {
                    _verticalVelocity = Mathf.Sqrt(_config.jumpHeight * -2f * _config.gravity);
                    _intent.Jump = false;
                }                
            }
            
            else _verticalVelocity += _config.gravity * Time.deltaTime;
                
            var velocity = moveDirection * currentSpeed + Vector3.up * _verticalVelocity;
            _controller.Move(velocity * Time.deltaTime);
        }

        public void OnMove(InputValue value) 
            => _intent.Move = value.Get<Vector2>();

        public void OnJump(InputValue value)
        {
            if (value.isPressed)
                _intent.Jump = true;
        }

        public void OnSprint(InputValue value)
        {
            _intent.Sprint = value.isPressed;
        }
    }
}