using System;
using _Project.Scripts.Domain.Gameplay.Configs;
using _Project.Scripts.Gameplay.Enemy.States;
using Fusion;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace _Project.Scripts.Gameplay.Enemy
{
    public class EnemyBrain : NetworkBehaviour
    {
        [Inject] private EnemyConfig _config;
        
        [Networked] private float NetworkSpeed { get; set; }
        [Networked] private TickTimer SearchCooldownTimer { get; set; }
        
        private static readonly Collider[] RunTimeBuffer = new Collider[16];
        
        private NavMeshAgent _agent;
        private EnemyState _currentState;
        private Transform _nearestPlayer;
        private EnemyCombat _combat;

        public IdleEnemyState IdleEnemyState { get; private set; }
        public ChaseEnemyState ChaseEnemyState { get; private set; }
        public AttackEnemyState AttackEnemyState { get; private set; }
        public Transform Target { get; private set; }

        public float DistanceToTarget => Target == null 
            ? Mathf.Infinity 
            : Vector3.Distance(transform.position, Target.position);
        public EnemyCombat Combat => _combat;
        public float AttackRange => _combat != null ? _combat.AttackRange : 0f;
        public float AttackCooldown => _combat != null ? _combat.AttackCooldown : 0f;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            
            if (_agent != null)
            {
                _agent.updatePosition = false;
                _agent.updateRotation = false;
            }

            if (_combat == null)
                _combat = GetComponent<EnemyCombat>();

            if (_agent == null)
                Debug.LogError("EnemyBrain: NavMeshAgent not found!");
            if (_combat == null)
                Debug.LogError("EnemyBrain: EnemyCombat not found!");

            IdleEnemyState = new IdleEnemyState(this);
            ChaseEnemyState = new ChaseEnemyState(this);
            AttackEnemyState = new AttackEnemyState(this);
        }

        public override void Spawned()
        {
            if (Runner != null && Runner.IsRunning && !HasStateAuthority)
            {
                DisableBrainLocally();
                return;
            }

            if (Object.HasStateAuthority && _agent != null)
            {
                _agent.Warp(transform.position);

                if (NetworkSpeed > 0) _agent.speed = NetworkSpeed;
            }
            
            ChangeState(IdleEnemyState); 
        }

        private void DisableBrainLocally()
        {
            if (_agent != null) _agent.enabled = false;
            if (_combat != null) _combat.enabled = false;
            
            enabled = false;
        }
        
        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority || _agent == null || !_agent.enabled) return;

            _currentState?.Update();
            
            var nextPosition = _agent.nextPosition;
            transform.position = nextPosition;
            _agent.velocity = _agent.desiredVelocity; 
            
            if (_agent.desiredVelocity.sqrMagnitude > 0.01f)
            {
                var targetRotation = Quaternion.LookRotation(_agent.desiredVelocity.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Runner.DeltaTime * 10f);
            }
        }

        public void ChangeState(EnemyState newState)
        {
            _currentState?.Exit();
            _currentState = newState;
            _currentState?.Enter();
        }

        public void SetTarget(Transform target)
        {
            Target = target;
            _nearestPlayer = target;
            SearchCooldownTimer = TickTimer.None;
        }

        public void StopAgent()
        {
            if (_agent != null && _agent.isOnNavMesh)
                _agent.isStopped = true;
        }

        public void ResumeAgent()
        {
            if (_agent != null && _agent.isOnNavMesh)
                _agent.isStopped = false;
        }

        public void SetDestination(Vector3 destination)
        {
            if (_agent != null && _agent.isOnNavMesh)
                _agent.SetDestination(destination);
        }

        public void FaceTarget()
        {
            if (Target == null) return;
            var dir = (Target.position - transform.position).normalized;
            dir.y = 0;
            if (dir.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.LookRotation(dir);
        }

        public void SetSpeed(float speed)
        {
            if (Object != null && Object.HasStateAuthority)
            {
                NetworkSpeed = speed;
            }

            if (_agent != null)
            {
                _agent.speed = speed;
            }
        }

        public void OnDeath()
        {
            enabled = false;
            StopAgent();
        }

        public Transform FindNearestPlayer()
        {
            if (IsSearchOnCooldown())
                return _nearestPlayer;

            SearchCooldownTimer = TickTimer.CreateFromSeconds(Runner, _config.searchIntervalSeconds);

            var candidate = FindClosestPlayerInRadius(out var candidateSqrDistance);
            
            _nearestPlayer = ResolveTarget(candidate, candidateSqrDistance);
            return _nearestPlayer;
        }

        private bool IsSearchOnCooldown()
            => SearchCooldownTimer.IsRunning && !SearchCooldownTimer.Expired(Runner);

        private Transform FindClosestPlayerInRadius(out float minSqrDistance)
        {
            minSqrDistance = Mathf.Infinity;

            Array.Clear(RunTimeBuffer, 0, RunTimeBuffer.Length);

            var hitCount = Physics.OverlapSphereNonAlloc(
                transform.position, _config.detectionRadius, RunTimeBuffer, _config.playerLayer);

            if (hitCount == 0)
                return null;

            Transform nearest = null;
            var origin = transform.position;

            for (var i = 0; i < hitCount; i++)
            {
                var hit = RunTimeBuffer[i];
                if (hit == null) continue;

                var sqrDist = (hit.transform.position - origin).sqrMagnitude;
                if (sqrDist >= minSqrDistance) continue;

                minSqrDistance = sqrDist;
                nearest = hit.transform;
            }

            return nearest;
        }

        private Transform ResolveTarget(Transform candidate, float candidateSqrDistance)
        {
            if (candidate == null || _nearestPlayer == null || candidate == _nearestPlayer)
                return candidate;

            var origin = transform.position;
            var currentSqr = (_nearestPlayer.position - origin).sqrMagnitude;

            if (currentSqr > _config.detectionRadius * _config.detectionRadius)
                return candidate;

            var thresholdSqr = currentSqr * _config.targetSwitchThreshold * _config.targetSwitchThreshold;
            return candidateSqrDistance < thresholdSqr ? candidate : _nearestPlayer;
        }
    }
}