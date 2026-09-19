using System;
using _Project.Scripts.Core.Data;
using _Project.Scripts.Domain.Gameplay.Data;
using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Scripts.Gameplay.Weapons
{
    public class WeaponController : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform shootOrigin;
        [SerializeField] private WeaponBase[] weapons;

        [Networked, OnChangedRender(nameof(RefreshWeaponVisuals))]
        private int _currentWeaponIndex { get; set; } = -1;
        
        [Networked] private NetworkButtons _oldButtons { get; set; }
        
        private WeaponBase _currentWeapon;
        private PlayerIntent _intent;
        private bool _firePressedThisFrame;
        
        public PlayerIntent Intent
        {
            get => _intent;
            set => _intent = value;
        }
        public int CurrentIndex => _currentWeaponIndex;

        public int TotalWeapons => weapons.Length;

        public override void Spawned()
        {
            InitializeWeapons();
            
            if (Runner.IsServer && weapons.Length > 0)
            {
                _currentWeaponIndex = 0;
            }
            RefreshWeaponVisuals();
        }

        private void InitializeWeapons()
        {
            if (shootOrigin == null)
                Debug.LogError("WeaponController: Shoot origin is not assigned!");
            
            if (weapons == null || weapons.Length == 0)
            {
                Debug.LogError("WeaponController: No weapons assigned in array!");
                return;
            }

            foreach (var weapon in weapons)
            {
                if (weapon == null) continue;
                weapon.Initialize(shootOrigin);
                weapon.gameObject.SetActive(false);
            }
        }

        private void Start()
        {
            if (Runner == null || !Runner.IsRunning)
            {
                InitializeWeapons();
                if (weapons is { Length: > 0 })
                {
                    SwitchWeaponLocal(0);
                }
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (GetInput(out NetworkInputData input))
            {
                var pressed = input.Buttons.GetPressed(_oldButtons);
                
                // === Network Weapon Switching ===
                if (input.WeaponSwitchIndex >= 0)
                {
                    ProcessWeaponSwitchCommand(input.WeaponSwitchIndex); 
                }

                if (_currentWeapon == null)
                {
                    _oldButtons = input.Buttons;
                    return;
                }

                // === Network Reload ===
                var reloadPressedThisTick = pressed.IsSet(PlayerButtons.Reload);
                if (reloadPressedThisTick)
                {
                    _currentWeapon.Reload();
                }

                // === Network Firing ===
                if (_currentWeapon.CanFire())
                {
                    var firePressedThisTick = pressed.IsSet(PlayerButtons.Fire);
                    var isFireHeld = input.Buttons.IsSet(PlayerButtons.Fire);
                    var shouldFire = _currentWeapon.IsAutomatic ? isFireHeld : firePressedThisTick;

                    if (shouldFire)
                    {
                        _currentWeapon.TryFire();
                    }
                }
                
                _oldButtons = input.Buttons;
            }
        }

        private void ProcessWeaponSwitchCommand(int command)
        {
            if (weapons == null || weapons.Length == 0) return;
            
            var total = weapons.Length;
            var nextIndex = _currentWeaponIndex;

            switch (command)
            {
                // "NextWeapon"
                case 100:
                    nextIndex = (_currentWeaponIndex + 1) % total;
                    break;
                // "PreviousWeapon"
                case 101:
                    nextIndex = (_currentWeaponIndex - 1 + total) % total;
                    break;
                default:
                {
                    if (command < total)
                    {
                        nextIndex = command;
                    }

                    break;
                }
            }

            if (nextIndex >= 0 && nextIndex < total)
            {
                _currentWeaponIndex = nextIndex;
            }
        }

        private void Update()
        {
            if (Runner == null || !Runner.IsRunning)
            {
                ApplyOfflineWeapons();
            }
        }

        private void ApplyOfflineWeapons()
        {
            if (weapons == null || weapons.Length == 0) return;
            
            // === Weapon Switching ===
            if (_intent.WeaponSwitchIndex >= 0)
            {
                ProcessWeaponSwitchCommand(_intent.WeaponSwitchIndex);
                SwitchWeaponLocal(_currentWeaponIndex);
                _intent.WeaponSwitchIndex = -1;
            }
            
            // === Reloading ===
            if(_intent.Reload && _currentWeapon != null)
            {
                _currentWeapon.Reload();
                _intent.Reload = false;
            }

            // === Firing ===
            if (_currentWeapon != null && _currentWeapon.CanFire())
            {
                var shouldFire = _currentWeapon.IsAutomatic ? _intent.Fire : _firePressedThisFrame;
                if (shouldFire)
                {
                    _currentWeapon.TryFire();
                    if(!_currentWeapon.IsAutomatic)
                        _firePressedThisFrame = false;
                }
            }
            
            if(!_currentWeapon.IsAutomatic && !_firePressedThisFrame)
                _firePressedThisFrame = false;
        }

        public void SwitchWeaponLocal(int newIndex)
        {
            if (weapons == null || weapons.Length == 0) return;
            if (newIndex < 0 || newIndex >= weapons.Length) return;

            foreach (var weapon in weapons)
            {
                if (weapon != null) weapon.gameObject.SetActive(false);
            }
            
            _currentWeaponIndex = newIndex;
            _currentWeapon = weapons[_currentWeaponIndex];
            _currentWeapon.gameObject.SetActive(true);
        }

        public Animator GetCurrentWeaponAnimator() => _currentWeapon != null ? _currentWeapon.GetComponent<Animator>() : null;

        public Transform GetShootOrigin() => shootOrigin;

        private void RefreshWeaponVisuals()
        {
            if (weapons == null || weapons.Length == 0) return;
            
            foreach (var weapon in weapons)
            {
                if (weapon != null) weapon.gameObject.SetActive(false);
            }

            if (_currentWeaponIndex >= 0 && _currentWeaponIndex < weapons.Length)
            {
                _currentWeapon = weapons[_currentWeaponIndex];
                _currentWeapon?.gameObject.SetActive(true);
            }
        }
    }
}