using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PQHD
{
    public class SceneSwitcher : MonoBehaviour
    {
        private static SceneSwitcher I;

        public enum Transition
        {
            HardCut,
            Diamonds,
        }

        [SerializeField] private RawImage overlay;

        [SerializeField] private float diamondsDuration = 0.5f;

        public static bool Transitioning => transitioning;
        private static bool transitioning;
        private static Transition activeTransition;

        

        void Awake()
        {
            I = this;
            if(transitioning)
            {
                StartCoroutine(TransitionInCoroutine(activeTransition));
            }            
        }
        

        public static void SwitchScenes(string newScene, Transition transition)
        {
            if(I == null) SceneManager.LoadScene(newScene); // fallback

            if(transitioning) return;
            I.SwitchScenesInternal(newScene, transition);
        }

        private void SwitchScenesInternal(string newScene, Transition transition)
        {
            if(transitioning) return;
            transitioning = true;
            activeTransition = transition;
            StartCoroutine(TransitionOutCoroutine(newScene, transition));
        }

        IEnumerator TransitionOutCoroutine(string newScene, Transition transition)
        {
            overlay.gameObject.SetActive(true);
            switch(transition)
            {
                case Transition.HardCut:
                    yield return null;
                    break;
                case Transition.Diamonds:
                    float t = 0; 
                    while(t < 1)
                    {
                        t += Time.deltaTime / diamondsDuration;
                        overlay.color = new Color(0,0,0,t);
                        yield return null;
                    }
                    overlay.color = Color.black;
                    yield return null;
                    break;
            }

            SceneManager.LoadScene(newScene);
        }

        IEnumerator TransitionInCoroutine(Transition transition)
        {
            overlay.gameObject.SetActive(true);
            
            // delay a few frames for initialization
            for(int i = 0; i < 5; i++) yield return null;
            // wait for saving/loading to finish
            while(Saving.Busy) yield return null;

            switch(transition)
            {
                case Transition.HardCut:
                    for(int i = 0; i < 5; i++) yield return null;
                    break;
                case Transition.Diamonds:
                    float t = 1; 
                    while(t > 0)
                    {
                        t -= Time.deltaTime / diamondsDuration;
                        overlay.color = new Color(0,0,0,t);
                        yield return null;
                    }
                    overlay.color = Color.clear;
                    yield return null;
                    break;
            }
            overlay.gameObject.SetActive(false);
            transitioning = false;
        }
    }
}
