using _Project.Scripts.Core.Signals;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Presentation.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class VictoryView : MonoBehaviour
    {
        [Inject] private SignalBus _signalBus;

        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            ApplyVisible(false);
        }

        private void Start()
        {
            _signalBus.Subscribe<VictoryStartedSignal>(OnVictory);
            _signalBus.Subscribe<ReturnToMainMenuForcedSignal>(OnForcedExit);
        }

        private void OnDestroy()
        {
            _signalBus.TryUnsubscribe<VictoryStartedSignal>(OnVictory);
            _signalBus.TryUnsubscribe<ReturnToMainMenuForcedSignal>(OnForcedExit);
        }

        private void OnVictory(VictoryStartedSignal _) => ApplyVisible(true);
        private void OnForcedExit(ReturnToMainMenuForcedSignal _) => ApplyVisible(false);

        public void OnExitButtonClicked()
        {
            _signalBus.Fire<ExitToMainMenuRequestedSignal>();
        }

        private void ApplyVisible(bool visible)
        {
            _canvasGroup.alpha = visible ? 1f : 0f;
            _canvasGroup.interactable = visible;
            _canvasGroup.blocksRaycasts = visible;

            if (!visible) return;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}