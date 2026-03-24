using System.Collections;
using PQHD.Dialog;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PQHD
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] GameObject splashScreen;
        [SerializeField] float splashScreenDuration;
        [SerializeField] SimpleUINavigator navigator;
        [SerializeField] SimpleUISelectable continueButton;
        [SerializeField] SimpleUISelectable newGameButton;
        [SerializeField] DialogGraphRuntime overwriteDialog;
        [SerializeField] private string gameScene;
        [SerializeField] private string introScene;


        [SerializeField] private RectTransform menuRootRect;
        [SerializeField] private StarfieldRenderer starfield;

        [SerializeField] private float starfieldScrollInSpeed = 5;
        


        bool HasSaveFile => Saving.SaveFileExists();

        void Start()
        {
            continueButton.Active = HasSaveFile;
            newGameButton.validateSelect = ValidateNewGameButtonPressed;
            StartCoroutine(IntroCoroutine());
        }

        public void ContinueGameButton()
        {
            SceneSwitcher.SwitchScenes(gameScene, SceneSwitcher.Transition.HardCut);
        }

        public bool ValidateNewGameButtonPressed()
        {
            if(HasSaveFile)
            {
                overwriteDialog.Play();
                navigator.enabled = false;
                StartCoroutine(ReenableNavigator());
                return false;
            }
            return true;
        }

        IEnumerator ReenableNavigator()
        {
            while(overwriteDialog.isPlaying) yield return null;
            navigator.enabled = true;
        }

        public void NewGameButton()
        {
            StartNewGame();
        }

        public void DeleteSaveAndNewGame()
        {
            Saving.DeleteSave();
            StartNewGame();
        }

        private void StartNewGame()
        {
            SceneSwitcher.SwitchScenes(introScene, SceneSwitcher.Transition.HardCut);
        }

        IEnumerator IntroCoroutine()
        {
            navigator.enabled = false;
            
            splashScreen.SetActive(true);
            yield return new WaitForSeconds(splashScreenDuration);
            splashScreen.SetActive(false);
            
            Vector3 starfieldBaseScroll = starfield.scroll;

            starfield.scroll = starfieldBaseScroll + new Vector3(0, starfieldScrollInSpeed, 0);
            menuRootRect.anchoredPosition = new Vector2(0, menuRootRect.rect.height);

            yield return new WaitForSeconds(0.25f);

            float t = 0;
            while(t < 1)
            {
                t += Time.deltaTime;

                float offset = (1 - t) * (1 - t);
                menuRootRect.anchoredPosition = new Vector2(0, offset * menuRootRect.rect.height);
                float offsetSpeed = 1 - t;
                starfield.scroll = starfieldBaseScroll + new Vector3(0, offsetSpeed * starfieldScrollInSpeed, 0);

                yield return null;
            }
            
            starfield.scroll = starfieldBaseScroll;
            menuRootRect.anchoredPosition = Vector3.zero;

            navigator.enabled = true;
        }
    }
}
