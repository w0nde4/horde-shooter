using System.Collections.Generic;
using _Project.Scripts.Core.Interfaces;
using _Project.Scripts.Core.Signals;
using _Project.Scripts.Services;
using Fusion;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Presentation.UI
{
    public class EnemyHealthBarHandler : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private EnemyHealthBarView barPrefab;
        [SerializeField] private Transform poolRoot;

        [Header("Placement")]
        [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2f, 0f);
        [SerializeField] private Camera worldCamera;

        [Inject] private SignalBus _signalBus;
        [Inject] private PlayerRegistry _playerRegistry;

        private readonly Dictionary<IEnemyHealth, EnemyHealthBarView> _active = new();
        private readonly Queue<EnemyHealthBarView> _pool = new();
        private readonly List<IEnemyHealth> _iterationBuffer = new();

        private void Awake()
        {
            if (poolRoot == null) poolRoot = transform;
        }

        private void OnEnable()
        {
            _signalBus.Subscribe<EnemySpawnedSignal>(OnEnemySpawned);
            _signalBus.Subscribe<EnemyDespawnedSignal>(OnEnemyDespawned);

            _playerRegistry.OnPlayerRegistered += OnPlayerRegistered;
            _playerRegistry.OnPlayerUnregistered += OnPlayerUnregistered;

            TryResolveCameraFromPlayers();
        }

        private void OnDisable()
        {
            _signalBus.Unsubscribe<EnemySpawnedSignal>(OnEnemySpawned);
            _signalBus.Unsubscribe<EnemyDespawnedSignal>(OnEnemyDespawned);

            _playerRegistry.OnPlayerRegistered -= OnPlayerRegistered;
            _playerRegistry.OnPlayerUnregistered -= OnPlayerUnregistered;

            _iterationBuffer.Clear();
            _iterationBuffer.AddRange(_active.Keys);
            foreach (var enemy in _iterationBuffer)
                if (_active.TryGetValue(enemy, out var bar))
                    ReturnToPool(bar);
            _active.Clear();
        }

        private void LateUpdate()
        {
            if (worldCamera == null)
            {
                TryResolveCameraFromPlayers();
                if (worldCamera != null)
                {
                    foreach (var bar in _active.Values)
                        bar.SetCamera(worldCamera);
                }
                if (worldCamera == null) return;
            }

            _iterationBuffer.Clear();
            _iterationBuffer.AddRange(_active.Keys);

            foreach (var enemy in _iterationBuffer)
            {
                if (enemy?.Transform == null) continue;
                if (!_active.TryGetValue(enemy, out var bar) || bar == null) continue;

                bar.SetWorldPosition(enemy.Transform.position + worldOffset);
            }
        }

        private void OnPlayerRegistered(GameObject player)
        {
            if (worldCamera != null) return;
            TryResolveCameraFromPlayers();

            if (worldCamera != null)
            {
                foreach (var bar in _active.Values)
                    bar.SetCamera(worldCamera);
            }
        }

        private void OnPlayerUnregistered(GameObject player)
        {
            if (worldCamera == null) return;
            if (worldCamera.transform.IsChildOf(player.transform) || worldCamera.gameObject == player)
                worldCamera = null;
        }

        private void TryResolveCameraFromPlayers()
        {
            var players = _playerRegistry.Players;
            if (players == null) return;

            foreach (var player in players)
            {
                if (player == null) continue;

                var no = player.GetComponent<NetworkObject>();
                if (no == null || !no.HasInputAuthority) continue;

                var cam = player.GetComponentInChildren<Camera>(includeInactive: true);
                if (cam != null)
                {
                    worldCamera = cam;
                    return;
                }
            }
        }

        private void OnEnemySpawned(EnemySpawnedSignal signal)
        {
            var enemy = signal.Enemy;
            if (enemy == null || _active.ContainsKey(enemy)) return;

            if (worldCamera == null) TryResolveCameraFromPlayers();

            var bar = GetFromPool();
            bar.Bind(enemy, worldCamera);

            if (worldCamera != null)
                bar.SetWorldPosition(enemy.Transform.position + worldOffset);

            _active[enemy] = bar;
        }

        private void OnEnemyDespawned(EnemyDespawnedSignal signal)
        {
            var enemy = signal.Enemy;
            if (enemy == null || !_active.TryGetValue(enemy, out var bar)) return;

            _active.Remove(enemy);
            ReturnToPool(bar);
        }

        private EnemyHealthBarView GetFromPool()
        {
            EnemyHealthBarView bar;
            if (_pool.Count > 0)
                bar = _pool.Dequeue();
            else
                bar = Instantiate(barPrefab, poolRoot);

            bar.transform.SetParent(poolRoot, worldPositionStays: false);
            bar.gameObject.SetActive(true);
            return bar;
        }

        private void ReturnToPool(EnemyHealthBarView bar)
        {
            if (bar == null) return;
            bar.Unbind();
            bar.gameObject.SetActive(false);
            bar.transform.SetParent(poolRoot, worldPositionStays: false);
            _pool.Enqueue(bar);
        }
    }
}