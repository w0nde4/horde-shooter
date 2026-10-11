using _Project.Scripts.Core.Signals;
using _Project.Scripts.Domain.Gameplay.Signals;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Presentation.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class PauseMenuView : MonoBehaviour
    {
        [SerializeField] private bool controlCursor = true;

        [Inject] private SignalBus _signalBus;

        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            ApplyVisible(false);
        }

        private void Start()
        {
            _signalBus.Subscribe<PauseMenuToggledSignal>(OnPauseToggled);
            _signalBus.Subscribe<ReturnToMainMenuForcedSignal>(OnForcedExit);
        }

        private void OnDestroy()
        {
            _signalBus.TryUnsubscribe<PauseMenuToggledSignal>(OnPauseToggled);
            _signalBus.TryUnsubscribe<ReturnToMainMenuForcedSignal>(OnForcedExit);
        }

        private void OnPauseToggled(PauseMenuToggledSignal signal)
        {
            ApplyVisible(signal.IsOpen);
        }

        private void OnForcedExit(ReturnToMainMenuForcedSignal _)
        {
            ApplyVisible(false);
            SetCursorForMenu();
        }

        public void OnExitButtonClicked()
        {
            SetCursorForMenu();
            _signalBus.Fire<ExitToMainMenuRequestedSignal>();
        }

        private void ApplyVisible(bool visible)
        {
            _canvasGroup.alpha = visible ? 1f : 0f;
            _canvasGroup.interactable = visible;
            _canvasGroup.blocksRaycasts = visible;

            if (!controlCursor) return;
            if (visible) SetCursorForMenu();
            else SetCursorForGameplay();
        }

        private void SetCursorForMenu()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void SetCursorForGameplay()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}