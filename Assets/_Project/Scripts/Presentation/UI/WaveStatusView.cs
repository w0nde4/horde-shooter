using System;
using System.Threading;
using _Project.Scripts.Core.Data;
using _Project.Scripts.Core.Signals;
using _Project.Scripts.Services.Network;
using Cysharp.Threading.Tasks;
using Fusion;
using TMPro;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Presentation.UI
{
    public class WaveStatusView : MonoBehaviour
    {
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private GameObject waitingPanel;

        [Inject] private SignalBus _signalBus;
        [Inject] private FusionNetworkService _networkService;

        private CancellationTokenSource _countdownCts;

        private void Awake()
        {
            if (waitingPanel != null) waitingPanel.SetActive(false);
        }

        private void Start()
        {
            _signalBus.Subscribe<WaveStateChangedSignal>(OnStateChanged);
        }

        private void OnDestroy()
        {
            _signalBus.TryUnsubscribe<WaveStateChangedSignal>(OnStateChanged);

            _countdownCts?.Cancel();
            _countdownCts?.Dispose();
            _countdownCts = null;
        }

        private void OnStateChanged(WaveStateChangedSignal signal)
        {
            _countdownCts?.Cancel();
            _countdownCts?.Dispose();
            _countdownCts = null;
            
            if (waitingPanel != null) waitingPanel.SetActive(false);

            switch (signal.State)
            {
                case WaveState.Intermission:
                    SetText("State: Intermission", signal.WaveNumber);
                    StartCountdown(signal.Duration, signal.WaveNumber, "Intermission");
                    break;

                case WaveState.Countdown:
                    SetText("State: Countdown", signal.WaveNumber);
                    StartCountdown(signal.Duration, signal.WaveNumber, "Countdown");
                    break;

                case WaveState.Fighting:
                    SetText("State: Fighting", signal.WaveNumber);
                    break;

                case WaveState.Upgrade:
                    SetText("State: Upgrade", signal.WaveNumber);
                    if (waitingPanel != null && IsMultiplayer()) waitingPanel.SetActive(true);
                    break;

                case WaveState.Victory:
                    SetText("State: Victory", signal.WaveNumber);
                    break;
            }
        }

        private void StartCountdown(float seconds, int waveNumber, string label)
        {
            _countdownCts = new CancellationTokenSource();
            RunCountdownAsync(seconds, waveNumber, label, _countdownCts.Token).Forget();
        }

        private async UniTaskVoid RunCountdownAsync(float seconds, int waveNumber, string label, CancellationToken ct)
        {
            var remaining = Mathf.CeilToInt(seconds);

            while (remaining > 0 && !ct.IsCancellationRequested)
            {
                SetText($"State: {label} ({remaining})", waveNumber);
                await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: ct);
                remaining--;
            }
        }

        private void SetText(string stateLine, int waveNumber)
        {
            if (statusText == null) return;
            statusText.text = $"{stateLine}\nWave: {waveNumber}";
        }
        
        private bool IsMultiplayer()
        {
            var runner = _networkService.Runner;
            return runner != null && runner.IsRunning && runner.GameMode != GameMode.Single;
        }
    }
}