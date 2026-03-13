 using System;
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
        
        [SerializeField] private InputStyle style;

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

        public static InputStyle Style { get; private set; } = InputStyle.Classic;
        public static Button ButtonA { get; private set; } = new();
        public static Button ButtonB { get; private set; } = new();
        public static Button ButtonStart { get; private set; } = new();
        
        public static Axis2 MoveAxis { get; private set; } = new();
        public static Axis2 AimAxis { get; private set; } = new();

        private void Update()
        {
            Style = style;

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
        }
    }
}
