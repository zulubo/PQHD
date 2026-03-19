using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace PQHD
{
    public class CrashCutscene : MonoBehaviour
    {
        /// <summary>
        /// play next time we load the crash site scene
        /// </summary>
        public static bool active;

        [SerializeField] private bool test;

        [SerializeField] private Transform crashedShip;
        [SerializeField] private float shipStartHeight = 50;
        [SerializeField] private float shipFallDuration = 0.5f;
        [SerializeField] private ParticleSystem crashParticles;
        [SerializeField] private AudioResource crashSound;
        [SerializeField] private GameObject music;
        [SerializeField] private float cameraShakeAmp = 0.3f;
        [SerializeField] private float cameraShakeDuration = 1f;

        void Start()
        {
            #if UNITY_EDITOR
            if(test) active = true;
            #endif

            if(active)
            {
                active = false;
                StartCoroutine(CutsceneCoroutine());
            }
        }

        private IEnumerator CutsceneCoroutine()
        {
            music.SetActive(false);
            GameObject player = Player.I.gameObject;
            player.SetActive(false);
            Vector3 shipPos = crashedShip.position;
            crashedShip.position = shipPos + Vector3.up * shipStartHeight;
            yield return new WaitForSeconds(0.5f);

            float fall = 1;
            while(fall > 0)
            {
                crashedShip.position = shipPos + Vector3.up * shipStartHeight * fall;
                fall -= Time.deltaTime;
                yield return null;
            }

            crashedShip.position = shipPos;
            crashParticles.Play();
            if(crashSound != null) Audio.I.PlaySound3D(crashSound, shipPos, 100);
            if(CameraFollow.I) CameraFollow.I.StartShake(cameraShakeDuration, cameraShakeAmp);

            yield return new WaitForSeconds(2f);

            player.SetActive(true);
            music.SetActive(true);
        }
    }
}
