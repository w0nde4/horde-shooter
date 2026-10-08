using _Project.Scripts.Core.Interfaces;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Presentation.UI
{
    public class EnemyHealthBarView : MonoBehaviour
    {
                [Header("UI Components")]
        [SerializeField] private Image healthFillImage;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Juice & Animation Settings")]
        [SerializeField] private float fillDuration = 0.15f;
        [SerializeField] private Ease fillEase = Ease.OutCubic;
        [SerializeField] private Vector3 damagePunchScale = new Vector3(0.15f, 0.15f, 0f);
        [SerializeField] private float punchDuration = 0.15f;

        private IEnemyHealth _trackedEnemy;
        private Camera _camera;

        private RectTransform _rect;
        private RectTransform _parentRect;

        private Tween _fillTween;
        private Tween _punchTween;
        private float _lastProcessedHealth = -1f;

        public IEnemyHealth TrackedEnemy => _trackedEnemy;

        private void Awake()
        {
            _rect = (RectTransform)transform;
        }

        public void Bind(IEnemyHealth enemy, Camera worldCamera)
        {
            Unbind();

            _trackedEnemy = enemy;
            _camera = worldCamera;
            _parentRect = _rect.parent as RectTransform;

            _trackedEnemy.OnHealthChanged += UpdateHealthVisuals;

            gameObject.SetActive(true);

            if (healthFillImage != null) healthFillImage.fillAmount = 1f;
            transform.localScale = Vector3.one;
            if (canvasGroup != null) canvasGroup.alpha = 1f;

            _lastProcessedHealth = -1f;
        }

        public void Unbind()
        {
            if (_trackedEnemy != null)
                _trackedEnemy.OnHealthChanged -= UpdateHealthVisuals;

            _trackedEnemy = null;
            _camera = null;
            _parentRect = null;
            _lastProcessedHealth = -1f;

            KillAllTweens();
        }

        public void SetWorldPosition(Vector3 worldAnchor)
        {
            if (_camera == null || _rect == null) return;

            var screenPoint = _camera.WorldToScreenPoint(worldAnchor);

            if (screenPoint.z <= 0f)
            {
                if (canvasGroup != null) canvasGroup.alpha = 0f;
                return;
            }
            if (canvasGroup != null) canvasGroup.alpha = 1f;

            if (_parentRect != null &&
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _parentRect, screenPoint, null, out var localPoint))
            {
                _rect.anchoredPosition = localPoint;
            }
            else
            {
                _rect.position = new Vector3(screenPoint.x, screenPoint.y, 0f);
            }
        }

        private void UpdateHealthVisuals(float currentHealth, float maxHealth)
        {
            if (maxHealth <= 0f) return;

            if (healthFillImage != null)
            {
                var targetFill = currentHealth / maxHealth;
                _fillTween?.Kill();
                _fillTween = DOTween
                    .To(() => healthFillImage.fillAmount, x => healthFillImage.fillAmount = x,
                        targetFill, fillDuration)
                    .SetEase(fillEase);
            }

            if (currentHealth < _lastProcessedHealth && _lastProcessedHealth >= 0f)
            {
                _punchTween?.Kill();
                transform.localScale = Vector3.one;
                _punchTween = transform.DOPunchScale(damagePunchScale, punchDuration,
                    vibrato: 1, elasticity: 0.5f);
            }

            _lastProcessedHealth = currentHealth;
        }

        private void KillAllTweens()
        {
            _fillTween?.Kill();
            _punchTween?.Kill();
            transform.DOKill();
            transform.localScale = Vector3.one;
        }
    }
}