using _Project.Scripts.Core.Data;
using _Project.Scripts.Domain.Gameplay;
using _Project.Scripts.Domain.Gameplay.Configs;
using _Project.Scripts.Domain.Gameplay.Data;
using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace _Project.Scripts.Gameplay.Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInputHandler))]
    public class PlayerController : NetworkBehaviour
    {
        [Inject] private PlayerConfig _config;
        [Inject] private PlayerStats _stats;
    
        private CharacterController _controller;
        private PlayerInputHandler _inputHandler;
        private float _verticalVelocity;
        
        [Networked] private NetworkButtons _oldButtons { get; set; }

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
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
            
            var moveDirection = transform.right * input.Move.x + transform.forward * input.Move.y;

            if (_controller.isGrounded)
            {
                if (_verticalVelocity < 0) _verticalVelocity = -2f;
                
                if (pressed.IsSet(PlayerButtons.Jump))
                {
                    _verticalVelocity = Mathf.Sqrt(_config.jumpHeight * -2f * _config.gravity);
                }                
            }
            else
            {
                _verticalVelocity += _config.gravity * Runner.DeltaTime;
            }
                
            var velocity = moveDirection * currentSpeed + Vector3.up * _verticalVelocity;
            _controller.Move(velocity * Runner.DeltaTime);
        }

        // Логика перемещения для ОФФЛАЙН режима (оригинальный ваш код)
        private void ApplyOfflineMovement()
        {
            var intent = _inputHandler.LocalOfflineIntent;
            var baseSpeed = _stats?.MoveSpeed ?? _config.defaultMoveSpeed; 
            var currentSpeed = intent.Sprint ? baseSpeed * _config.sprintMultiplier : baseSpeed;
            
            var moveDirection = transform.right * intent.Move.x + transform.forward * intent.Move.y;

            if (_controller.isGrounded)
            {
                if (_verticalVelocity < 0) _verticalVelocity = -2f;

                if (intent.Jump)
                {
                    _verticalVelocity = Mathf.Sqrt(_config.jumpHeight * -2f * _config.gravity);
                    // Так как структура PlayerIntent возвращается по значению из свойства,
                    // сброс флага здесь оффлайн-интенту не поможет, но оффлайн использует WasPressedThisFrame, который сбросится сам.
                }                
            }
            else
            {
                _verticalVelocity += _config.gravity * Time.deltaTime;
            }
                
            var velocity = moveDirection * currentSpeed + Vector3.up * _verticalVelocity;
            _controller.Move(velocity * Time.deltaTime);
        }

        //todo: delete it after bridge done
        // public void OnMove(InputValue value) 
        //     => _intent.Move = value.Get<Vector2>();
        //
        // public void OnJump(InputValue value)
        // {
        //     if (value.isPressed)
        //         _intent.Jump = true;
        // }
        //
        // public void OnSprint(InputValue value)
        // {
        //     _intent.Sprint = value.isPressed;
        // }
    }
}