using UnityEngine;
using UnityEngine.Playables;

namespace PQHD
{
    public class IntroCutscene : MonoBehaviour
    {
        [SerializeField] PlayableDirector timeline;
        [SerializeField] string nextScene;

        void Start()
        {
            
        }

        void Update()
        {
            if(!done && (!timeline.playableGraph.IsPlaying() || timeline.time > timeline.playableAsset.duration - 0.1f))
            {
                LoadNextScene();
            }
        }

        bool done;

        void LoadNextScene()
        {
            if(done) return;
            done = true;
            CrashCutscene.active = true;
            SceneSwitcher.SwitchScenes(nextScene, SceneSwitcher.Transition.HardCut);
        }
    }
}
