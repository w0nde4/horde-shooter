using _Project.Scripts.Core.Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Scripts.Gameplay.Weapons
{
    public class WeaponController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform shootOrigin;
        [SerializeField] private WeaponBase[] weapons;

        private WeaponBase _currentWeapon;
        private int _currentWeaponIndex = -1;
        private PlayerIntent _intent;
        private bool _firePressedThisFrame;
        
        public PlayerIntent Intent
        {
            get => _intent;
            set => _intent = value; 
        }
        public int CurrentIndex => _currentWeaponIndex;
        public int TotalWeapons => weapons.Length;

        private void Start()
        {
            if (shootOrigin == null)
                Debug.LogError("WeaponController: Shoot origin is not assigned!");

            foreach (var weapon in weapons)
            {
                if (weapon == null) continue;
                weapon.Initialize(shootOrigin);
                weapon.gameObject.SetActive(false);
            }
        
            if (weapons.Length > 0)
            {
                SwitchWeapon(0);
            }
            else
            {
                Debug.LogError("WeaponController: No weapons assigned!");
            }
        }

        private void Update()
        {
            // === Weapon Switching ===
            if (_intent.WeaponSwitchIndex >= 0)
            {
                SwitchWeapon(_intent.WeaponSwitchIndex);
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

        //todo: delete it after bridge done
        // public void OnFire(InputValue value)
        // {
        //     var pressed = value.isPressed;
        //     _intent.Fire = pressed;
        //     if (value.isPressed)
        //         _firePressedThisFrame = true;
        // }
        //
        // public void OnReload(InputValue value)
        // {
        //     if (value.isPressed)
        //         _intent.Reload = true;
        // }
        //
        // public void OnWeaponOne(InputValue value)
        // {
        //     if (value.isPressed) _intent.WeaponSwitchIndex = 0;
        // }
        //
        // public void OnWeaponTwo(InputValue value)
        // {
        //     if (value.isPressed) _intent.WeaponSwitchIndex = 1;
        // }
        //
        // public void OnNextWeapon(InputValue value)
        // {
        //     if (!value.isPressed) return;
        //
        //     var nextIndex = (_currentWeaponIndex + 1) % weapons.Length;
        //     _intent.WeaponSwitchIndex = nextIndex;
        // }
        //
        // public void OnPreviousWeapon(InputValue value)
        // {
        //     if (!value.isPressed) return;
        //
        //     var prevIndex = (_currentWeaponIndex - 1 + weapons.Length) % weapons.Length;
        //     _intent.WeaponSwitchIndex = prevIndex;
        // }

        public void SwitchWeapon(int newIndex)
        {
            if (weapons == null || weapons.Length == 0) return;
            if (newIndex < 0 || newIndex >= weapons.Length) return;
            if (newIndex == _currentWeaponIndex && _currentWeapon != null) return;
        
            if (_currentWeapon != null)
                _currentWeapon.gameObject.SetActive(false);

            _currentWeaponIndex = newIndex;
            _currentWeapon = weapons[_currentWeaponIndex];
            _currentWeapon.gameObject.SetActive(true);
        }
    }
}