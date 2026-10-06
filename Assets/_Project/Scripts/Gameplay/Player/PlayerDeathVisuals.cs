using _Project.Scripts.Core.Interfaces;
using Fusion;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Player
{
    [RequireComponent(typeof(PlayerHealth))]
    public class PlayerDeathVisuals : NetworkBehaviour
    {
        [Header("Visuals")]
        [SerializeField] private Renderer[] renderers;

        [Header("Physics")]
        [SerializeField] private Collider[] collidersToDisable;
        [SerializeField] private CharacterController characterController;

        private IPlayerHealth _health;
        
        public override void Spawned()
        {
            _health = GetComponent<IPlayerHealth>();

            if (_health == null)
            {
                Debug.LogError($"[PlayerDeathVisuals] No IPlayerHealth found on {name}");
                enabled = false;
                return;
            }

            if (renderers == null || renderers.Length == 0)
                renderers = GetComponentsInChildren<Renderer>(true);

            _health.OnDeath += HandleDeath;
            _health.OnRespawned += HandleRespawn;

            Apply(!_health.IsDead);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (_health != null)
            {
                _health.OnDeath -= HandleDeath;
                _health.OnRespawned -= HandleRespawn;
            }
            base.Despawned(runner, hasState);
        }

        private void HandleDeath() => Apply(false);
        private void HandleRespawn() => Apply(true);

        private void Apply(bool isAlive)
        {
            if (renderers != null)
                foreach (var r in renderers)
                    if (r != null) r.enabled = isAlive;

            if (collidersToDisable != null)
                foreach (var c in collidersToDisable)
                    if (c != null) c.enabled = isAlive;

            if (characterController != null)
                characterController.enabled = isAlive;
        }
    }
}