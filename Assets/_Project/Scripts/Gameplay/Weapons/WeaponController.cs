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
        private int CurrentWeaponIndex { get; set; } = -1;
        
        [Networked] private NetworkButtons OldButtons { get; set; }
        
        private WeaponBase _currentWeapon;
        
        public int CurrentIndex => CurrentWeaponIndex;
        public int TotalWeapons => weapons.Length;

        public override void Spawned()
        {
            InitializeWeapons();
            
            if (Object.HasStateAuthority && weapons.Length > 0)
            {
                CurrentWeaponIndex = 0;
                _currentWeapon = weapons[0];
                _currentWeapon.gameObject.SetActive(true);
            }
            else
            {
                RefreshWeaponVisuals();
            }
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

        public override void FixedUpdateNetwork()
        {
            if (GetInput(out NetworkInputData input))
            {
                var pressed = input.Buttons.GetPressed(OldButtons);
                
                // === Network Weapon Switching ===
                if (input.WeaponSwitchIndex >= 0)
                {
                    ProcessWeaponSwitchCommand(input.WeaponSwitchIndex); 
                }

                if (_currentWeapon == null)
                {
                    OldButtons = input.Buttons;
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
                
                OldButtons = input.Buttons;
            }
        }

        private void ProcessWeaponSwitchCommand(int command)
        {
            if (weapons == null || weapons.Length == 0) return;
            
            var total = weapons.Length;
            var nextIndex = CurrentWeaponIndex;

            switch (command)
            {
                // "NextWeapon"
                case 100:
                    nextIndex = (CurrentWeaponIndex + 1) % total;
                    break;
                // "PreviousWeapon"
                case 101:
                    nextIndex = (CurrentWeaponIndex - 1 + total) % total;
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

            if (nextIndex >= 0 && nextIndex < total && nextIndex != CurrentWeaponIndex)
            {
                CurrentWeaponIndex = nextIndex;
                
                if (Object.HasStateAuthority)
                {
                    UpdateLocalWeaponInstance(nextIndex);
                }
            }
        }
        
        private void UpdateLocalWeaponInstance(int index)
        {
            foreach (var weapon in weapons)
            {
                if (weapon != null) weapon.gameObject.SetActive(false);
            }

            _currentWeapon = weapons[index];
            if (_currentWeapon != null)
            {
                _currentWeapon.gameObject.SetActive(true);
            }
        }

        private void RefreshWeaponVisuals()
        {
            if (weapons == null || weapons.Length == 0) return;
            
            UpdateLocalWeaponInstance(CurrentWeaponIndex);
        }

        public Animator GetCurrentWeaponAnimator() => _currentWeapon != null ? _currentWeapon.GetComponent<Animator>() : null;
        public Transform GetShootOrigin() => shootOrigin;
        public Transform GetMuzzleOrigin() => _currentWeapon != null ? _currentWeapon.MuzzleTransform : null;
    }
}