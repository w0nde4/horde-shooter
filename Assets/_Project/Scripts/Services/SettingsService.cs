using _Project.Scripts.Core.Signals;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Services
{
    public class SettingsService : IInitializable
    {
        private const string MasterVolumeKey = "settings.audio.master";
        private const string FullscreenModeKey = "settings.video.fullscreenMode";
        private const string FpsCapKey = "settings.video.fpsCap";
        private const string FovKey = "settings.gameplay.fov";
        private const string InvertYKey = "settings.gameplay.invertY";

        public float MasterVolume { get; private set; }
        public FullScreenMode FullscreenMode { get; private set; }
        public int FpsCap { get; private set; }
        public float Fov { get; private set; }
        public bool InvertY { get; private set; }

        [Inject] private SignalBus _signalBus;

        public void Initialize()
        {
            MasterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
            FullscreenMode = (FullScreenMode)PlayerPrefs.GetInt(FullscreenModeKey, (int)FullScreenMode.FullScreenWindow);
            FpsCap = PlayerPrefs.GetInt(FpsCapKey, 144);
            Fov = PlayerPrefs.GetFloat(FovKey, 75f);
            InvertY = PlayerPrefs.GetInt(InvertYKey, 0) == 1;

            ApplyGlobal();
            FireGameplaySignal();
        }

        public void SetMasterVolume(float value)
        {
            MasterVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(MasterVolumeKey, MasterVolume);
            PlayerPrefs.Save();
            ApplyGlobal();
        }

        public void SetFullscreenMode(FullScreenMode mode)
        {
            FullscreenMode = mode;
            PlayerPrefs.SetInt(FullscreenModeKey, (int)mode);
            PlayerPrefs.Save();
            ApplyGlobal();
        }

        public void SetFpsCap(int cap)
        {
            FpsCap = cap;
            PlayerPrefs.SetInt(FpsCapKey, cap);
            PlayerPrefs.Save();
            ApplyGlobal();
        }

        public void SetFov(float fov)
        {
            Fov = Mathf.Clamp(fov, 60f, 120f);
            PlayerPrefs.SetFloat(FovKey, Fov);
            PlayerPrefs.Save();
            FireGameplaySignal();
        }

        public void SetInvertY(bool invert)
        {
            InvertY = invert;
            PlayerPrefs.SetInt(InvertYKey, invert ? 1 : 0);
            PlayerPrefs.Save();
            FireGameplaySignal();
        }

        private void ApplyGlobal()
        {
            AudioListener.volume = MasterVolume;
            Screen.fullScreenMode = FullscreenMode;
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = FpsCap;
        }

        private void FireGameplaySignal()
        {
            _signalBus.Fire(new GameplaySettingsChangedSignal(Fov, InvertY));
        }
    }
}