using System.Collections.Generic;
using _Project.Scripts.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace _Project.Scripts.Presentation.UI
{
    public class SettingsView : MonoBehaviour
    {
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Toggle invertYToggle;
        [SerializeField] private Slider fovSlider;
        [SerializeField] private TMP_Dropdown fullscreenModeDropdown;
        [SerializeField] private TMP_Dropdown fpsCapDropdown;
        [SerializeField] private Button closeButton;
        [SerializeField] private TMP_Text masterVolumeValueText;
        [SerializeField] private TMP_Text fovValueText;

        [Inject] private SettingsService _settingsService;

        // Индексы совпадают с порядком в дропдауне.
        private static readonly FullScreenMode[] FullscreenModes =
        {
            FullScreenMode.Windowed,
            FullScreenMode.FullScreenWindow, // Borderless
            FullScreenMode.ExclusiveFullScreen
        };

        private static readonly int[] FpsCaps = { 30, 60, 120, 144, -1 };

        private void Start()
        {
            masterVolumeSlider.SetValueWithoutNotify(_settingsService.MasterVolume);
            invertYToggle.SetIsOnWithoutNotify(_settingsService.InvertY);
            fovSlider.SetValueWithoutNotify(_settingsService.Fov);
            fullscreenModeDropdown.SetValueWithoutNotify(IndexOf(FullscreenModes, _settingsService.FullscreenMode));
            fpsCapDropdown.SetValueWithoutNotify(IndexOf(FpsCaps, _settingsService.FpsCap));
            fullscreenModeDropdown.RefreshShownValue();
            fpsCapDropdown.RefreshShownValue();
            
            UpdateMasterVolumeText(_settingsService.MasterVolume);
            UpdateFovText(_settingsService.Fov);

            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            invertYToggle.onValueChanged.AddListener(OnInvertYChanged);
            fovSlider.onValueChanged.AddListener(OnFovChanged);
            fullscreenModeDropdown.onValueChanged.AddListener(OnFullscreenModeChanged);
            fpsCapDropdown.onValueChanged.AddListener(OnFpsCapChanged);
        }

        private void OnDestroy()
        {
            masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
            invertYToggle.onValueChanged.RemoveListener(OnInvertYChanged);
            fovSlider.onValueChanged.RemoveListener(OnFovChanged);
            fullscreenModeDropdown.onValueChanged.RemoveListener(OnFullscreenModeChanged);
            fpsCapDropdown.onValueChanged.RemoveListener(OnFpsCapChanged);
        }
        
        private void UpdateMasterVolumeText(float value)
        {
            if (masterVolumeValueText != null)
                masterVolumeValueText.text = $"{Mathf.RoundToInt(value * 100f)}%";
        }

        private void UpdateFovText(float value)
        {
            if (fovValueText != null)
                fovValueText.text = Mathf.RoundToInt(value).ToString();
        }

        private void OnMasterVolumeChanged(float value)
        {
            _settingsService.SetMasterVolume(value);
            UpdateMasterVolumeText(value);
        }

        private void OnFovChanged(float value)
        {
            _settingsService.SetFov(value);
            UpdateFovText(value);
        }
        private void OnInvertYChanged(bool value) => _settingsService.SetInvertY(value);

        private void OnFullscreenModeChanged(int index)
        {
            if (index < 0 || index >= FullscreenModes.Length) return;
            _settingsService.SetFullscreenMode(FullscreenModes[index]);
        }

        private void OnFpsCapChanged(int index)
        {
            if (index < 0 || index >= FpsCaps.Length) return;
            _settingsService.SetFpsCap(FpsCaps[index]);
        }

        private static int IndexOf<T>(T[] array, T value)
        {
            for (var i = 0; i < array.Length; i++)
                if (EqualityComparer<T>.Default.Equals(array[i], value)) return i;
            return 0;
        }
    }
}