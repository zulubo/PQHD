using System.Collections;
using PQHD.Dialog;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PQHD
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] SimpleUINavigator navigator;
        [SerializeField] SimpleUISelectable continueButton;
        [SerializeField] SimpleUISelectable newGameButton;

        [SerializeField] DialogGraphRuntime overwriteDialog;

        [SerializeField] private string gameScene;

        bool HasSaveFile => Saving.SaveFileExists();

        void Start()
        {
            continueButton.Active = HasSaveFile;
            newGameButton.validateSelect = ValidateNewGameButtonPressed;
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
            SceneSwitcher.SwitchScenes(gameScene, SceneSwitcher.Transition.HardCut);
        }
    }
}
