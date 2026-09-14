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

        [Networked] private float _networkSpeed { get; set; } 
        
        private NavMeshAgent _agent;
        private EnemyState _currentState;

        public IdleEnemyState IdleEnemyState { get; private set; }
        public ChaseEnemyState ChaseEnemyState { get; private set; }
        public AttackEnemyState AttackEnemyState { get; private set; }
        public Transform Target { get; private set; }

        public float DistanceToTarget => Target == null ? Mathf.Infinity : Vector3.Distance(transform.position, Target.position);
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
                if (_agent != null) _agent.enabled = false;
                if (combat != null) combat.enabled = false;
                if(TryGetComponent<EnemyHealth>(out var health))
                {
                    health.enabled = false;
                }

                enabled = false;
                return;
            }

            if (Object.HasStateAuthority && _agent != null && _networkSpeed > 0)
            {
                _agent.speed = _networkSpeed;
            }
            
            ChangeState(IdleEnemyState); 
        }

        private void Start()
        {
            if (Runner == null || !Runner.IsRunning)
            {
                ChangeState(IdleEnemyState);
            }
        }

        private void Update()
        {
            _currentState?.Update();
        }
        
        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority || _agent == null || !_agent.enabled) return;

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

        public void SetTarget(Transform target) => Target = target;

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
            var hits = Physics.OverlapSphere(transform.position, detectionRadius, playerLayer);
            Transform nearest = null;
            var minDistance = Mathf.Infinity;

            foreach (var hit in hits)
            {
                Debug.Log($"Found player: {hit.name} at distance {Vector3.Distance(transform.position, hit.transform.position)}");
                var dist = Vector3.Distance(transform.position, hit.transform.position);
                if (!(dist < minDistance)) continue;
                minDistance = dist;
                nearest = hit.transform;
            }

            return nearest;
        }

        public void SetSpeed(float speed)
        {
            if (Object != null && Object.HasStateAuthority)
            {
                _networkSpeed = speed;
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