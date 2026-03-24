using UnityEngine;
using TMPro;
using UnityEngine.Audio;
using System.Collections;


namespace PQHD
{
    public class PauseMenu : MonoBehaviour
    {
        public static PauseMenu I;
        private bool paused;
        public static bool Paused => I && I.paused;

        [SerializeField] GameObject root;

        [SerializeField] TMP_Text headerText;

        [System.Serializable]
        public class Section
        {
            public string id;
            public string displayName;
            public GameObject gameObject;
            public string parentID;
        }

        public Section[] sections;
        private int activeSection;

        [SerializeField] private AudioMixerGroup musicMixer;

        [SerializeField] private AudioResource pauseSound;
        [SerializeField] private AudioResource backSound;

        public void SetSection(string id)
        {
            int newSection = 0;
            
            for(int i = 0; i < sections.Length; i ++)
            {
                if(sections[i].id == id)
                {
                    newSection = i;
                    break;  
                } 
            }

            if(newSection != activeSection)
            {
                activeSection = newSection;
                UpdateSectionsActive();    
            }
        }

        private void UpdateSectionsActive()
        {
            for(int i = 0; i < sections.Length; i ++)
            {
                sections[i].gameObject.SetActive(i == activeSection);
            }
            headerText.text = sections[activeSection].displayName;
        }

        public void Pause()
        {
            activeSection = 0;
            UpdateSectionsActive();
            root.SetActive(true);
            Time.timeScale = 0;
            musicMixer.audioMixer.SetFloat("MusicPitch", 0);
            Audio.I.PlaySound2D(pauseSound);
            paused = true;
        }

        public void UnPause()
        {
            root.SetActive(false);
            Time.timeScale = 1;
            musicMixer.audioMixer.SetFloat("MusicPitch", 1);
            paused = false;
        }

        void Start()
        {
            I = this;
        }

        void OnDestroy()
        {
            if(paused) UnPause();
        }

        void Update()
        {
            if(Input.ButtonStart.WasPressedThisFrame)
            {
                if(paused) UnPause();
                else Pause();
            }

            if(paused && Input.ButtonB.WasPressedThisFrame)
            {
                if(!string.IsNullOrEmpty(sections[activeSection].parentID))
                {
                    SetSection(sections[activeSection].parentID);
                    Audio.I.PlaySound2D(backSound);
                }
                else
                {
                    StartCoroutine(DelayUnpause());
                }
            }
        }

        IEnumerator DelayUnpause()
        {
            yield return null;
            UnPause();
        }

        public void ResetGame()
        {
            SceneSwitcher.SwitchScenes("MainMenu", SceneSwitcher.Transition.HardCut);    
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}
