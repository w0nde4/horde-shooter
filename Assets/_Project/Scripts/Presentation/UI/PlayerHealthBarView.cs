using _Project.Scripts.Domain.Gameplay;
using _Project.Scripts.Gameplay.Player;
using _Project.Scripts.Services;
using DG.Tweening;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace _Project.Scripts.Presentation.UI
{
    public class PlayerHealthBarView : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Image healthFillImage;
        [SerializeField] private TMP_Text healthText;
        
        [Header("Juice & Animation Settings")]
        [SerializeField] private float fillDuration = 0.2f;
        [SerializeField] private Ease fillEase = Ease.OutCubic;
        [SerializeField] private Vector3 damagePunchScale = new Vector3(0.15f, 0.15f, 0f);
        [SerializeField] private float punchDuration = 0.15f;

        [Inject] private PlayerRegistry _playerRegistry;
        [Inject] private SignalBus _signalBus;

        private PlayerHealth _trackedPlayerHealth;
        private PlayerStats _trackedPlayerStats;
        
        private Tween _fillTween;
        private Tween _punchTween;
        private float _lastProcessedHealth = -1f;

        private void OnEnable()
        {
            _playerRegistry.OnPlayerRegistered += OnPlayerRegistered;
            _playerRegistry.OnPlayerUnregistered += OnPlayerUnregistered;

            if (_playerRegistry.TryGetPlayer("LocalPlayer", out var playerGo))
            {
                BindPlayer(playerGo);
            }
        }

        private void OnDisable()
        {
            _playerRegistry.OnPlayerRegistered -= OnPlayerRegistered;
            _playerRegistry.OnPlayerUnregistered -= OnPlayerUnregistered;
            
            UnbindPlayer();
            KillAllTweens();
        }

        private void OnPlayerRegistered(GameObject playerGameObject)
        {
            if (playerGameObject == null) return;

            var networkObject = playerGameObject.GetComponent<NetworkObject>();
            if (networkObject != null && networkObject.HasInputAuthority)
            {
                BindPlayer(playerGameObject);
            }
        }

        private void OnPlayerUnregistered(GameObject playerGameObject)
        {
            if (playerGameObject != null && playerGameObject == _trackedPlayerHealth?.gameObject)
            {
                UnbindPlayer();
            }
        }

        private void BindPlayer(GameObject playerGameObject)
        {
            UnbindPlayer();

            _trackedPlayerHealth = playerGameObject.GetComponent<PlayerHealth>();
            
            var context = playerGameObject.GetComponent<GameObjectContext>();
            if (context != null)
            {
                _trackedPlayerStats = context.Container.Resolve<PlayerStats>();
            }

            if (_trackedPlayerHealth != null)
            {
                _trackedPlayerHealth.OnHealthChanged += UpdateHealthVisuals;
                
                var maxHealth = _trackedPlayerStats?.MaxHealth ?? 100f;
                _lastProcessedHealth = maxHealth;
                
                UpdateHealthVisualsInstant(maxHealth, maxHealth);
            }
        }

        private void UnbindPlayer()
        {
            if (_trackedPlayerHealth != null)
            {
                _trackedPlayerHealth.OnHealthChanged -= UpdateHealthVisuals;
            }
            _trackedPlayerHealth = null;
            _trackedPlayerStats = null;
            _lastProcessedHealth = -1f;
        }

        private void UpdateHealthVisuals(float currentHealth, float maxHealth)
        {
            if (maxHealth <= 0f) return;

            if (healthFillImage != null)
            {
                var targetFill = currentHealth / maxHealth;
                
                _fillTween?.Kill();
                _fillTween = DOTween.To(() => healthFillImage.fillAmount, x => healthFillImage.fillAmount = x, targetFill, fillDuration).SetEase(fillEase);
            }

            if (currentHealth < _lastProcessedHealth && _lastProcessedHealth >= 0f)
            {
                _punchTween?.Kill();
                transform.localScale = Vector3.one;
                
                _punchTween = transform.DOPunchScale(damagePunchScale, punchDuration, vibrato: 1, elasticity: 0.5f);
            }

            if (healthText != null)
            {
                var current = Mathf.CeilToInt(Mathf.Max(0f, currentHealth));
                var max = Mathf.CeilToInt(maxHealth);
                healthText.text = $"{current}/{max}";
            }

            _lastProcessedHealth = currentHealth;
        }

        private void UpdateHealthVisualsInstant(float currentHealth, float maxHealth)
        {
            KillAllTweens();

            if (healthFillImage != null) healthFillImage.fillAmount = currentHealth / maxHealth;
            if (healthText != null) healthText.text = $"{Mathf.CeilToInt(currentHealth)}/{Mathf.CeilToInt(maxHealth)}";
            
            transform.localScale = Vector3.one;
            _lastProcessedHealth = currentHealth;
        }

        private void KillAllTweens()
        {
            _fillTween?.Kill();
            _punchTween?.Kill();
            transform.DOKill();
        }
    }
}