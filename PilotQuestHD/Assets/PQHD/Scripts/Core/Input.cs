 using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PQHD
{
    [DefaultExecutionOrder(-100)]
    public class Input : MonoBehaviour
    {
        [SerializeField] private InputActionReference action_A;
        [SerializeField] private InputActionReference action_B;
        [SerializeField] private InputActionReference action_B_Alt;
        [SerializeField] private InputActionReference action_Start;
        [SerializeField] private InputActionReference action_Move;
        [SerializeField] private InputActionReference action_Aim;

        [SerializeField] private float stickDeadzone = 0.4f;

        public enum InputStyle
        {
            Classic,
            Modern
        }
        
        public class Button
        {
            public bool Pressed { get; private set; }
            public bool WasPressedThisFrame { get; private set; }
            public bool WasReleasedThisFrame { get; private set; }

            private bool oldValue;

            public void Update(bool value)
            {
                Pressed = value;
                WasPressedThisFrame = value && !oldValue;
                WasReleasedThisFrame = !value && oldValue;
                oldValue = value;
            }
        }

        public class Axis2
        {
            public Vector2 Position { get; private set; } = new();
            
            public Button DPadDown { get; private set; } = new();
            public Button DPadUp { get; private set; } = new();
            public Button DPadLeft { get; private set; } = new();
            public Button DPadRight { get; private set; } = new();

            private const float dpadThreshold = 0.8f;
            public void Update(Vector2 position, float deadzone = 0)
            {
                if (position.magnitude > deadzone)
                {
                    if (Style == InputStyle.Classic)
                    {
                        position = position.normalized;
                        position.x = Mathf.Round(position.x);
                        position.y = Mathf.Round(position.y);
                    }

                    position = Vector2.ClampMagnitude(position, 1);
                }
                else
                {
                    position = Vector2.zero;
                }
                
                Position = position;

                DPadDown.Update(position.y < -dpadThreshold);
                DPadUp.Update(position.y > dpadThreshold);
                DPadLeft.Update(position.x < -dpadThreshold);
                DPadRight.Update(position.x > dpadThreshold);
            }
        }

        public static InputStyle Style { get; set; }
        public static bool VibrationEnabled { get; set; } = true;
        public static Button ButtonA { get; private set; } = new();
        public static Button ButtonB { get; private set; } = new();
        public static Button ButtonStart { get; private set; } = new();
        
        public static Axis2 MoveAxis { get; private set; } = new();
        public static Axis2 AimAxis { get; private set; } = new();

        public enum HapticFreq
        {
            Low,
            High
        }

        private class HapticEvent
        {
            private float duration;
            private float amplitude;
            private float timer;
            public HapticFreq freq;

            private const float DefaultFreq = 20;

            public HapticEvent(float duration, float amplitude, HapticFreq freq)
            {
                this.duration = duration;
                this.amplitude = amplitude;
                this.freq = freq;
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

        private static List<HapticEvent> haptics = new();

        public static void Haptics(float duration, float amplitude, HapticFreq freq)
        {
            if(!VibrationEnabled) return;

            haptics.Add(new HapticEvent(duration, amplitude, freq));
        }

        private void Update()
        {
            ButtonA.Update(action_A.action.IsPressed());
            ButtonStart.Update(action_Start.action.IsPressed());
            MoveAxis.Update(action_Move.action.ReadValue<Vector2>(), stickDeadzone);
            AimAxis.Update(action_Aim.action.ReadValue<Vector2>(), stickDeadzone);
            
            switch (Style)
            {
                case InputStyle.Classic:
                    AimAxis.Update(Vector2.zero);
                    ButtonB.Update(action_B.action.IsPressed());
                    break;
                case InputStyle.Modern:
                    AimAxis.Update(action_Aim.action.ReadValue<Vector2>(), stickDeadzone);
                    ButtonB.Update(action_B.action.IsPressed() || action_B_Alt.action.IsPressed());
                    break;
            }

            // haptics
            float lowFreqHaptics = 0;
            float highFreqHaptics = 0;
            for(int i = haptics.Count - 1; i >= 0; i--)
            {
                haptics[i].Update(Time.deltaTime, out bool finished, out float amp);
                
                if(finished)
                {
                    haptics.RemoveAt(i);
                    continue;
                }

                switch(haptics[i].freq)
                {
                    case HapticFreq.Low: 
                        lowFreqHaptics += amp;
                        break;
                    case HapticFreq.High:
                        highFreqHaptics += amp;
                        break;
                }
            }

            if(Gamepad.current != null)
            {
                Gamepad.current.SetMotorSpeeds(lowFreqHaptics, highFreqHaptics);
            }
        }

        void OnDestroy()
        {
            haptics.Clear();
            InputSystem.ResetHaptics();
        }
    }
}
