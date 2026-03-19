using System.Collections.Generic;
using PQHD.Dialog;
using UnityEngine;

namespace PQHD
{
    public class CameraFollow : MonoBehaviour
    {
        public static CameraFollow I;

        public Transform target;

        [SerializeField] private Vector2 deadZone;
        [SerializeField] private float horizontalSmoothing;
        [SerializeField] private float verticalSmoothing;

        private class ShakeEvent
        {
            private float duration;
            private float amplitude;
            public float frequency;
            public float seed;
            private float timer;

            private const float DefaultFreq = 20;

            public ShakeEvent(float duration, float amplitude)
            {
                this.duration = duration;
                this.amplitude = amplitude;
                this.frequency = DefaultFreq;
                seed = Random.Range(-10000,10000);
                timer = 0;
            }

            public ShakeEvent(float duration, float amplitude, float frequency)
            {
                this.duration = duration;
                this.amplitude = amplitude;
                this.frequency = frequency;
                seed = Random.Range(-10000,10000);
                timer = 0;
            }

            public void Update(float deltaTime, out bool finished, out float currentAmplitude)
            {
                timer += deltaTime;
                float falloff = 1 - (timer / duration);
                currentAmplitude = amplitude * falloff * falloff;
                finished = timer > duration;
            }
        }

        private List<ShakeEvent> shakes = new();

        public void StartShake(float duration, float amplitude)
        {
            shakes.Add(new ShakeEvent(duration, amplitude));
        }

        void Start()
        {
            I = this;
            TeleportToTarget();
        }

        private Vector3 position;

        private void Update()
        {
            Vector3 followPos = GetFollowPos();

            if (horizontalSmoothing > 0)
            {
                position.x = Mathf.Lerp(position.x, followPos.x, Time.deltaTime / horizontalSmoothing);
                position.z = Mathf.Lerp(position.z, followPos.z, Time.deltaTime / horizontalSmoothing);
            }
            else
            {
                position.x = followPos.x;
                position.z = followPos.z;
            }

            if (verticalSmoothing > 0)
            {
                position.y = Mathf.Lerp(position.y, followPos.y, Time.deltaTime / horizontalSmoothing);
            }
            else
            {
                position.y = followPos.y;
            }

            Vector3 processedPosition = position;

            // apply camera shake
            for(int i = shakes.Count - 1; i >= 0; i--)
            {
                shakes[i].Update(Time.deltaTime, out bool finished, out float amp);
                
                if(finished)
                {
                    shakes.RemoveAt(i);
                    continue;
                }

                processedPosition += new Vector3(Mathf.PerlinNoise1D((shakes[i].seed + Time.timeSinceLevelLoad) * shakes[i].frequency),
                                                 Mathf.PerlinNoise1D((shakes[i].seed + Time.timeSinceLevelLoad + 100) * shakes[i].frequency),
                                                 Mathf.PerlinNoise1D((shakes[i].seed + Time.timeSinceLevelLoad - 100) * shakes[i].frequency)) * amp;
            }

            transform.position = processedPosition;
        }

        private Vector3 GetFollowPos()
        {
            Vector3 followPos = target.position;

            if (DialogManager.IsPlayingDialog && DialogManager.ActiveDialogRuntime.pullCameraFocusWhenActive)
            {
                followPos = DialogManager.ActiveDialogRuntime.transform.position;
            }
            
            Vector3 followOffset = followPos - position;
            Vector3 followOffsetWithDeadzone = followOffset;
            followOffsetWithDeadzone.x = followOffset.x - Mathf.Clamp(followOffset.x, -deadZone.x, deadZone.x);
            followOffsetWithDeadzone.z = followOffset.z - Mathf.Clamp(followOffset.z, -deadZone.y, deadZone.y);

            followPos = position + followOffsetWithDeadzone;

            if (CameraLimits.Exists)
            {
                CameraLimits.Active.ClampPosition(ref followPos);
            }

            return followPos;
        }

        private void TeleportToTarget()
        {
            position = GetFollowPos();
            Update();
        }
    }
}