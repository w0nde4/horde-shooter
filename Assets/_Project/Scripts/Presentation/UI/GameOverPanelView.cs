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
            _signalBus.Subscribe<GameOverStartedSignal>(ShowGameOverWindow);
        }

        private void OnDisable()
        {
            _signalBus.TryUnsubscribe<GameOverStartedSignal>(ShowGameOverWindow);
            KillTweens();
        }

        private void ShowGameOverWindow()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            KillTweens();

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