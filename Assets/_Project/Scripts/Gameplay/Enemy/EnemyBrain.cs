using _Project.Scripts.Gameplay.Enemy.States;
using UnityEngine;
using UnityEngine.AI;

namespace _Project.Scripts.Gameplay.Enemy
{
    public class EnemyBrain : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private EnemyCombat combat;
        [SerializeField] private LayerMask playerLayer;
        [SerializeField] private float detectionRadius = 100f;

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

        private void Start()
        {
            ChangeState(IdleEnemyState);
        }

        private void Update()
        {
            _currentState?.Update();
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

        public void OnDeath()
        {
            enabled = false;
            StopAgent();
        }
    }
}