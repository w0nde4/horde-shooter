using System;
using _Project.Scripts.Gameplay.Enemy.States;
using Fusion;
using UnityEngine;
using UnityEngine.AI;

namespace _Project.Scripts.Gameplay.Enemy
{
    public class EnemyBrain : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private EnemyCombat combat;
        [SerializeField] private LayerMask playerLayer;
        [SerializeField] private float detectionRadius = 100f;
        [SerializeField] private float searchIntervalSeconds = 0.2f;

        [Header("Targeting")] 
        [Range(0.5f, 1f)] 
        [SerializeField] private float targetSwitchThreshold = 0.2f;
        
        [Networked] private float NetworkSpeed { get; set; }
        [Networked] private TickTimer SearchCooldownTimer { get; set; }
        
        private static readonly Collider[] RunTimeBuffer = new Collider[16];
        
        private NavMeshAgent _agent;
        private EnemyState _currentState;
        private Transform _nearestPlayer;

        public IdleEnemyState IdleEnemyState { get; private set; }
        public ChaseEnemyState ChaseEnemyState { get; private set; }
        public AttackEnemyState AttackEnemyState { get; private set; }
        public Transform Target { get; private set; }

        public float DistanceToTarget => Target == null 
            ? Mathf.Infinity 
            : Vector3.Distance(transform.position, Target.position);
        public EnemyCombat Combat => combat;
        public float AttackRange => combat != null ? combat.AttackRange : 0f;
        public float AttackCooldown => combat != null ? combat.AttackCooldown : 0f;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            
            if (_agent != null)
            {
                _agent.updatePosition = false;
                _agent.updateRotation = false;
            }

            if (combat == null)
                combat = GetComponent<EnemyCombat>();

            if (_agent == null)
                Debug.LogError("EnemyBrain: NavMeshAgent not found!");
            if (combat == null)
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
            if (combat != null) combat.enabled = false;
            
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

        public Transform FindNearestPlayer()
        {
            if (SearchCooldownTimer.IsRunning && !SearchCooldownTimer.Expired(Runner))
            {
                return _nearestPlayer;
            }

            SearchCooldownTimer = TickTimer.CreateFromSeconds(Runner, searchIntervalSeconds);
            Array.Clear(RunTimeBuffer, 0, RunTimeBuffer.Length);

            var hitCount = Physics.OverlapSphereNonAlloc(
                transform.position, detectionRadius, RunTimeBuffer, playerLayer);
            
            if (hitCount == 0)
            {
                _nearestPlayer = null;
                return null;
            }

            Transform nearest = null;
            var minSqrDistance = Mathf.Infinity;
            var currentPosition = transform.position;
            
            for (var i = 0; i < hitCount; i++)
            {
                var hit = RunTimeBuffer[i];
                if (hit == null) continue;

                var direction = hit.transform.position - currentPosition;
                var sqrDist = direction.sqrMagnitude;

                if (sqrDist < minSqrDistance)
                {
                    minSqrDistance = sqrDist;
                    nearest = hit.transform;
                }
            }

            if (_nearestPlayer != null && nearest != null && nearest != _nearestPlayer)
            {
                var currentDir = _nearestPlayer.position - currentPosition;
                var currentSqr = currentDir.sqrMagnitude;
                var maxSqr = detectionRadius * detectionRadius;

                if (currentSqr <= maxSqr)
                {
                    var thresholdSqr = currentSqr * targetSwitchThreshold * targetSwitchThreshold;
                    if (minSqrDistance >= thresholdSqr)
                    {
                        nearest = _nearestPlayer;
                    }
                }
            }
            
            _nearestPlayer = nearest;
            return nearest;
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
    }
}