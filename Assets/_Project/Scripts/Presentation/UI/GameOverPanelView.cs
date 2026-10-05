using _Project.Scripts.Core.Signals;
using _Project.Scripts.Gameplay.Player;
using _Project.Scripts.Services;
using _Project.Scripts.Services.Network;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Fusion;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace _Project.Scripts.Presentation.UI
{
    public class GameOverPanelView : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button mainMenuButton;

        [Header("Animation Settings")]
        [SerializeField] private Transform buttonsGroup;
        [SerializeField] private float fadeDuration = 0.4f;
        [SerializeField] private float buttonsScaleDuration = 0.3f;

        [Header("Scene Names")]
        [SerializeField] private string mainMenuSceneName = "StartMenu";

        [Inject] private PlayerRegistry _playerRegistry;
        [Inject] private FusionNetworkService _networkService;
        [Inject] private SignalBus _signalBus;

        private CanvasGroup _canvasGroup;
        private PlayerHealth _trackedPlayerHealth;
        private Tween _fadeTween;
        private Tween _scaleTween;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }

        private void OnEnable()
        {
            _playerRegistry.OnPlayerRegistered += OnPlayerRegistered;
            _playerRegistry.OnPlayerUnregistered += OnPlayerUnregistered;
        }

        private void OnDisable()
        {
            _playerRegistry.OnPlayerRegistered -= OnPlayerRegistered;
            _playerRegistry.OnPlayerUnregistered -= OnPlayerUnregistered;
            UnbindPlayer();
            KillTweens();
        }

        private void OnPlayerRegistered(GameObject playerGameObject)
        {
            var networkObject = playerGameObject.GetComponent<NetworkObject>();
            if (networkObject != null && networkObject.HasInputAuthority)
            {
                UnbindPlayer();
                _trackedPlayerHealth = playerGameObject.GetComponent<PlayerHealth>();
                if (_trackedPlayerHealth != null)
                {
                    _trackedPlayerHealth.OnDeath += ShowGameOverWindow;
                }
            }
        }

        private void OnPlayerUnregistered(GameObject playerGameObject)
        {
            if (playerGameObject == _trackedPlayerHealth?.gameObject)
            {
                UnbindPlayer();
            }
        }

        private void UnbindPlayer()
        {
            if (_trackedPlayerHealth != null)
            {
                _trackedPlayerHealth.OnDeath -= ShowGameOverWindow;
            }
            _trackedPlayerHealth = null;
        }

        private void ShowGameOverWindow()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            gameObject.SetActive(true);
            KillTweens();

            _signalBus.Fire<GameOverStartedSignal>();

            if (buttonsGroup != null) buttonsGroup.localScale = Vector3.zero;

            _fadeTween = DOTween.To(() => _canvasGroup.alpha, x => _canvasGroup.alpha = x, 1f, fadeDuration)
                .OnComplete(() =>
                {
                    _canvasGroup.interactable = true;
                    _canvasGroup.blocksRaycasts = true;

                    if (buttonsGroup != null)
                    {
                        _scaleTween = buttonsGroup.DOScale(Vector3.one, buttonsScaleDuration)
                            .SetEase(Ease.OutBack);
                    }
                });
        }

        private void OnMainMenuClicked()
        {
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            _networkService.ExitToMainMenuAsync(mainMenuSceneName).Forget();
        }

        private void KillTweens()
        {
            _fadeTween?.Kill();
            _scaleTween?.Kill();
        }   
    }
}