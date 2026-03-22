using System;
using UnityEngine;

namespace PQHD
{
    public class SettingsMenu : MonoBehaviour
    {
        public class Setting
        {
            public string id;
            public Action<int> write;
            public Func<int> read;

            public Setting(string id)
            {
                this.id = id;
            }
        }

        private readonly Setting[] settings = new Setting[]
        {
            new("inputStyle")
            {
                write = (i) =>
                {
                    Settings.I.settings.inputStyle = i == 1 ? Input.InputStyle.Modern : Input.InputStyle.Classic;
                },
                read = () =>
                {
                    return Settings.I.settings.inputStyle == Input.InputStyle.Modern ? 1 : 0;
                },
            },
            
            new("vibration")
            {
                write = (i) =>
                {
                    Settings.I.settings.vibration = i == 1;
                },
                read = () =>
                {
                    return Mathf.RoundToInt(Settings.I.settings.vibration ? 1 : 0);
                },
            },

            new("display")
            {
                write = (i) =>
                {
                    switch(i)
                    {
                        case 0: 
                            Settings.I.settings.displayMode = FullScreenMode.FullScreenWindow;
                            break;
                        case 1: 
                            Settings.I.settings.displayMode = FullScreenMode.ExclusiveFullScreen;
                            break;
                        case 2:
                            Settings.I.settings.displayMode = FullScreenMode.Windowed;
                            break;
                    }
                },

                read = () =>
                {
                    switch(Settings.I.settings.displayMode)
                    {
                        case FullScreenMode.FullScreenWindow: 
                            return 0;
                        case FullScreenMode.ExclusiveFullScreen: 
                            return 1;
                        case FullScreenMode.Windowed:
                            return 2;
                    }
                    return 0;
                },
            },

            new("audio_volume")
            {
                write = (i) =>
                {
                    Settings.I.settings.audio_volume = i / 10f;
                },
                read = () =>
                {
                    return Mathf.RoundToInt(Settings.I.settings.audio_volume * 10);
                },
            },
        
            new("audio_bgm")
            {
                write = (i) =>
                {
                    Settings.I.settings.audio_bgmEnabled = i == 1;
                },
                read = () =>
                {
                    return Mathf.RoundToInt(Settings.I.settings.audio_bgmEnabled ? 1 : 0);
                },
            },

            new("audio_sfx")
            {
                write = (i) =>
                {
                    Settings.I.settings.audio_sfxEnabled = i == 1;
                },
                read = () =>
                {
                    return Mathf.RoundToInt(Settings.I.settings.audio_sfxEnabled ? 1 : 0);
                },
            }
        };

        private bool TryFindSetting(string key, out Setting setting)
        {
            setting = Array.Find(settings, s => s.id == key);
            return setting != null;
        }

        // this is janky   
        public void SetSetting(string key, int value)
        {
            if(TryFindSetting(key, out Setting s))
            {
                s.write(value);
                Settings.I.ApplySettings();
                Settings.I.Save();
            }
        }

        public int GetSetting(string key)
        {
            if(TryFindSetting(key, out Setting s))
            {
                return s.read();
            }

            return 0;
        }
    }
}
