using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace PQHD
{
    public class Settings : MonoBehaviour
    {
        public static Settings I;

        [SerializeField] AudioMixerGroup audioMasterGroup;
        [SerializeField] AudioMixerGroup audioMusicGroup;
        [SerializeField] AudioMixerGroup audioSfxGroup;
        

        [System.Serializable]
        public struct SettingsContainer
        {
            public FullScreenMode displayMode;
            public Input.InputStyle inputStyle;
            public bool vibration;
            public float audio_volume;
            public bool audio_bgmEnabled;
            public bool audio_sfxEnabled;

            public static SettingsContainer Default()
            {
                return new SettingsContainer()
                {
                    displayMode = FullScreenMode.FullScreenWindow,
                    inputStyle = Input.InputStyle.Modern,
                    vibration = true,
                    audio_volume = 1,
                    audio_bgmEnabled = true,
                    audio_sfxEnabled = true,
                };
            }
        }

        public SettingsContainer settings = SettingsContainer.Default();

        void Start()
        {
            I = this;
            Saving.OnLoadedState += LoadedState;
            Saving.LoadFromDisk();
        }

        void OnDestroy()
        {
            Saving.OnLoadedState -= LoadedState;
        }

        public void Save()
        {
            Saving.State.settings = settings;
            Saving.SaveToDisk();
        }

        private void LoadedState(Saving.SaveState state)
        {
            settings = state.settings;
            ApplySettings();
        }

        public void ApplySettings()
        {
            Screen.fullScreenMode = settings.displayMode;
            Input.Style = settings.inputStyle;
            Input.VibrationEnabled = settings.vibration;
            SetVolume(audioMasterGroup, "MasterVolume", settings.audio_volume);
            SetVolume(audioMusicGroup, "MusicVolume", settings.audio_bgmEnabled ? 1 : 0);
            SetVolume(audioMasterGroup, "SFXVolume", settings.audio_sfxEnabled ? 1 : 0);
        }

        private static void SetVolume(AudioMixerGroup group, string parameter, float volume)
        {
            volume = Mathf.Max(volume, 0.0001f);
            group.audioMixer.SetFloat(parameter, Mathf.Log(volume) * 20);
        }
    }
}
