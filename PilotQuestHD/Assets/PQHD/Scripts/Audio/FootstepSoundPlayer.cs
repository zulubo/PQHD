using System;
using UnityEngine;

namespace PQHD
{
    public class FootstepSoundPlayer : MonoBehaviour
    {
        [SerializeField] CharacterController characterController;
        [SerializeField] private FootstepSoundProfile profile;
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private float volumeMultiplier = 1;

        [SerializeField] private bool considerSpeed = true;
        [Range(0, 0.9f)] [SerializeField] private float minSpeed = 0.25f;

        public float Speed { private get; set; }

        [SerializeField] private float radius = 10;


        public void PlayFootstepSound()
        {
            if (!isActiveAndEnabled) return;
            if (!characterController.isGrounded) return;
            if (DetectGround(out RaycastHit hit))
            {
                float volume = volumeMultiplier;
                if (considerSpeed)
                {
                    if (Speed < minSpeed) return;
                    volume *= Mathf.InverseLerp(minSpeed, 1, Speed);
                }
                
                profile.Play(hit.collider.sharedMaterial, transform.position, radius, volume);
            }
        }

        bool DetectGround(out RaycastHit hit)
        {
            return Physics.SphereCast(characterController.transform.TransformPoint(characterController.center),
                characterController.radius * 0.5f, Vector3.down, out hit, characterController.height, groundMask);
        }
    }
}