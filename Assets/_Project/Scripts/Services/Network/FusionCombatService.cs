using _Project.Scripts.Core.Interfaces;
using Fusion;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Services.Network
{
    public class FusionCombatService : IDamageService
    {
        [Inject] private CombatService _localCombatService;
        [Inject] private NetworkRunner _runner;
        public void ApplyDamage(float amount, IDamageable target, GameObject source = null)
        {
            ApplyDamage(amount, target, Vector3.zero, Vector3.zero, source);
        }

        public void ApplyDamage(float amount, IDamageable target, Vector3 hitPoint, Vector3 hitNormal, GameObject source = null)
        {
            if (_runner == null || !_runner.IsServer) 
                return;

            if (target == null) 
                return;

            _localCombatService.ApplyDamage(amount, target, hitPoint, hitNormal, source);
        }
    }
}