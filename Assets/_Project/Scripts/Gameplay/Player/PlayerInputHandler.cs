using System;
using System.Collections.Generic;
using _Project.Scripts.Core.Signals;
using _Project.Scripts.Domain.Gameplay.Data;
using _Project.Scripts.Domain.Gameplay.Signals;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace _Project.Scripts.Gameplay.Player
{
    public class PlayerInputHandler : NetworkBehaviour, INetworkRunnerCallbacks
    {
        [SerializeField] private InputActionAsset inputActionAsset; // InputActions file

        [Inject] private SignalBus _signalBus;
        
        private InputActionAsset _inputActions; // Instance
        private bool _isSelectionBlocked;
        private bool _isRespawnBlocked;
        private bool _isGameOverBlocked;
        private bool _isPaused;
        private bool _isVictoryBlocked;
        private bool IsInputBlocked => 
            _isSelectionBlocked 
            || _isRespawnBlocked 
            || _isGameOverBlocked 
            || _isPaused 
            || _isVictoryBlocked;

        private struct PlayerActions
        {
            public InputAction Move, Look, Jump, Sprint, Fire, Reload, Grenade;
            public InputAction WeaponOne, WeaponTwo, NextWeapon, PreviousWeapon;
            public InputAction Pause;
        }
        private PlayerActions _actions;

        // Network buffers
        private bool _jumpPressed;
        private bool _reloadPressed;
        private bool _grenadePressed;
        private Vector2 _rawMouseAccumulated;
        private int _selectedWeaponIndexCache = -1;

        public Vector2 RawMouseAccumulated => _rawMouseAccumulated;

        public override void Spawned()
        {
            if (HasInputAuthority)
            {
                EnsureInputActions();
                Runner.AddCallbacks(this);
                
                _signalBus.Subscribe<UpgradeSelectionStartedSignal>(OnSelectionStarted);
                _signalBus.Subscribe<UpgradeSelectionEndedSignal>(OnSelectionEnded);
                _signalBus.Subscribe<GameOverStartedSignal>(OnGameOverStarted);
                _signalBus.Subscribe<PlayerRespawnStartedSignal>(OnRespawnStarted);
                _signalBus.Subscribe<PlayerRespawnEndedSignal>(OnRespawnEnded);
                _signalBus.Subscribe<PauseMenuToggledSignal>(OnPauseMenuToggled);
                _signalBus.Subscribe<VictoryStartedSignal>(OnVictoryStarted);
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasReason)
        {
            if (HasInputAuthority && Runner != null)
            {
                runner.RemoveCallbacks(this);
                
                _signalBus.TryUnsubscribe<UpgradeSelectionStartedSignal>(OnSelectionStarted);
                _signalBus.TryUnsubscribe<UpgradeSelectionEndedSignal>(OnSelectionEnded);
                _signalBus.TryUnsubscribe<GameOverStartedSignal>(OnGameOverStarted);
                _signalBus.TryUnsubscribe<PlayerRespawnStartedSignal>(OnRespawnStarted);
                _signalBus.TryUnsubscribe<PlayerRespawnEndedSignal>(OnRespawnEnded);
                _signalBus.TryUnsubscribe<PauseMenuToggledSignal>(OnPauseMenuToggled);
                _signalBus.TryUnsubscribe<VictoryStartedSignal>(OnVictoryStarted);
            }
        }
        
        private void OnSelectionStarted() => _isSelectionBlocked = true;
        private void OnSelectionEnded() => _isSelectionBlocked = false;
        private void OnGameOverStarted() => _isGameOverBlocked = true;
        private void OnVictoryStarted() => _isVictoryBlocked = true;
        private void OnPauseMenuToggled(PauseMenuToggledSignal signal) => _isPaused = signal.IsOpen;

        private void OnRespawnStarted(PlayerRespawnStartedSignal signal)
        {
            if(Object.InputAuthority != signal.Player) return;
            _isRespawnBlocked = true;
            _rawMouseAccumulated = Vector2.zero;
        }
        
        private void OnRespawnEnded(PlayerRespawnEndedSignal signal)
        {
            if(Object.InputAuthority != signal.Player) return;
            _isRespawnBlocked = false;
            _rawMouseAccumulated = Vector2.zero;
        }

        private void Update()
        {
            if (!HasInputAuthority || _inputActions == null) return;
            
            if (_actions.Pause.WasPressedThisFrame())
            {
                _signalBus.Fire(new PauseMenuToggledSignal(!_isPaused));
            }

            if(IsInputBlocked)
            {
                _selectedWeaponIndexCache = -1;
                return;
            }
            
            _rawMouseAccumulated += _actions.Look.ReadValue<Vector2>();
        
            if (_actions.Jump.WasPressedThisFrame()) _jumpPressed = true;
            if (_actions.Reload.WasPressedThisFrame()) _reloadPressed = true;
            if (_actions.Grenade.WasPressedThisFrame()) _grenadePressed = true;
        
            if (_actions.WeaponOne.WasPressedThisFrame()) _selectedWeaponIndexCache = 0;
            else if (_actions.WeaponTwo.WasPressedThisFrame()) _selectedWeaponIndexCache = 1;
            else if (_actions.NextWeapon.triggered) _selectedWeaponIndexCache = 100;
            else if (_actions.PreviousWeapon.triggered) _selectedWeaponIndexCache = 101; 
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            if (!HasInputAuthority || _inputActions == null) return;

            if (IsInputBlocked)
            {
                var blankData = new NetworkInputData
                {
                    Move = Vector2.zero,
                    Look = _rawMouseAccumulated,
                    WeaponSwitchIndex = -1
                };
                
                input.Set(blankData);
        
                _jumpPressed = false;
                _reloadPressed = false;
                _grenadePressed = false;
                return;
            }
            
            var data = new NetworkInputData
            {
                Move = _actions.Move.ReadValue<Vector2>(),
                Look = _rawMouseAccumulated,
                WeaponSwitchIndex = GetWeaponSwitchIndex(),
            };
        
            data.Buttons.Set(PlayerButtons.Jump, _jumpPressed);
            data.Buttons.Set(PlayerButtons.Sprint, _actions.Sprint.IsPressed());
            data.Buttons.Set(PlayerButtons.Fire, _actions.Fire.IsPressed());
            data.Buttons.Set(PlayerButtons.Reload, _reloadPressed);
            data.Buttons.Set(PlayerButtons.Grenade, _grenadePressed);

            // Buffers Reset
            _jumpPressed = false;
            _reloadPressed = false;
            _grenadePressed = false;

            input.Set(data);
        }
    
        public float ClampVerticalBuffer(float minRawPitch, float maxRawPitch)
        {
            return _rawMouseAccumulated.y = Mathf.Clamp(_rawMouseAccumulated.y, minRawPitch, maxRawPitch);
        }
    
        private int GetWeaponSwitchIndex()
        {
            var command = _selectedWeaponIndexCache;
            _selectedWeaponIndexCache = -1; 
            return command;
        }

        private void EnsureInputActions()
        {
            if (_inputActions == null)
            {
                _inputActions = Instantiate(inputActionAsset);
            
                _actions.Move = _inputActions.FindAction("Move");
                _actions.Look = _inputActions.FindAction("Look");
                _actions.Jump = _inputActions.FindAction("Jump");
                _actions.Sprint = _inputActions.FindAction("Sprint");
                _actions.Fire = _inputActions.FindAction("Fire");
                _actions.Reload = _inputActions.FindAction("Reload");
                _actions.Grenade = _inputActions.FindAction("Grenade");
                _actions.WeaponOne = _inputActions.FindAction("WeaponOne");
                _actions.WeaponTwo = _inputActions.FindAction("WeaponTwo");
                _actions.NextWeapon = _inputActions.FindAction("NextWeapon");
                _actions.PreviousWeapon = _inputActions.FindAction("PreviousWeapon");
                _actions.Pause = _inputActions.FindAction("Pause");
            
                _inputActions.Enable();
            }
        }

        private void OnDisable()
        {
            if (_inputActions != null) _inputActions.Disable();
        }
    
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ReadOnlySpan<byte> data) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
    }
}