using System;
using UnityEngine;

namespace PQHD
{
    public class PlayerMover : MonoBehaviour
    {
        [SerializeField] CharacterController characterController;
        [SerializeField] private float gravity = 5;
        [SerializeField] private float moveSpeed;

        [SerializeField] private float rotateSmoothing;

        [SerializeField] FootstepSoundPlayer footstepSoundPlayer;

        private float facingAngle;

        public Vector2 MoveInput { get; private set; }

        public BoolTimer StopMoving;


        private void Start()
        {
            facingAngle = Mathf.Atan2(transform.forward.z, transform.forward.x);
        }


        private const bool ModernControls = true;

        bool AllowMovement => !StopMoving && !Dialog.DialogManager.IsPlayingDialog;

        private void Update()
        {
            Vector2 moveInput = Input.MoveAxis.Position;

            if (moveInput.sqrMagnitude > 0 && AllowMovement)
            {
                moveInput = Vector2.ClampMagnitude(moveInput, 1);

                Vector3 move = new Vector3(moveInput.x, 0, moveInput.y) * moveSpeed + Vector3.down * gravity;
                characterController.Move(move * Time.deltaTime);

                facingAngle = Mathf.Atan2(moveInput.y, moveInput.x);
            }
            else
            {
                moveInput = Vector2.zero;
            }

            if (ModernControls && AllowMovement)
            {
                Vector2 aimInput = Input.AimAxis.Position;
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