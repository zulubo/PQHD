using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PQHD
{
    public class PlayerMover : MonoBehaviour
    {
        [SerializeField] CharacterController characterController;
        [SerializeField] private float gravity = 5;
        [SerializeField] private float moveSpeed;

        private InputAction moveAction;
        private InputAction aimAction;

        [SerializeField] private float rotateSmoothing;

        [SerializeField] FootstepSoundPlayer footstepSoundPlayer;

        private float facingAngle;

        public Vector2 MoveInput { get; private set; }

        public BoolTimer StopMoving;


        private void Start()
        {
            moveAction = InputSystem.actions.FindAction("Move");
            aimAction = InputSystem.actions.FindAction("Aim");
            facingAngle = Mathf.Atan2(transform.forward.z, transform.forward.x);
        }


        private const bool ModernControls = true;

        private void Update()
        {
            Vector2 moveInput = moveAction.ReadValue<Vector2>();

            if (moveInput.magnitude > 0.5f && !StopMoving)
            {
                if (!ModernControls)
                {
                    moveInput = moveInput.normalized;
                    moveInput.x = Mathf.Round(moveInput.x);
                    moveInput.y = Mathf.Round(moveInput.y);
                }

                moveInput = Vector2.ClampMagnitude(moveInput, 1);

                Vector3 move = new Vector3(moveInput.x, 0, moveInput.y) * moveSpeed + Vector3.down * gravity;
                characterController.Move(move * Time.deltaTime);

                facingAngle = Mathf.Atan2(moveInput.y, moveInput.x);
            }
            else
            {
                moveInput = Vector2.zero;
            }

            if (ModernControls && !StopMoving)
            {
                Vector2 aimInput = aimAction.ReadValue<Vector2>();
                if (aimInput.magnitude > 0.2f)
                {
                    facingAngle = Mathf.Atan2(aimInput.y, aimInput.x);
                }
            }

            Quaternion facingRotation = Quaternion.Euler(0, -facingAngle * Mathf.Rad2Deg + 90, 0);
            if (rotateSmoothing > 0)
                transform.rotation =
                    Quaternion.Slerp(transform.rotation, facingRotation, Time.deltaTime / rotateSmoothing);
            else transform.localRotation = facingRotation;

            MoveInput = moveInput;

            footstepSoundPlayer.Speed = MoveInput.magnitude;
        }
    }
}