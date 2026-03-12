using System;
using UnityEngine;

namespace PQHD
{
    public class PlayerAnimator : MonoBehaviour
    {
        [SerializeField] private Player player;
        [SerializeField] private Transform tiltedTransform;
        [SerializeField] private float tiltAmount = 20;
        [SerializeField] private PlayerMover mover;
        [SerializeField] private Animator anim;
        [SerializeField] private float dampTime = 0.1f;

        private readonly int anim_moveSpeed = Animator.StringToHash("MoveSpeed");
        private static readonly int anim_YoyoAttack = Animator.StringToHash("YoyoAttack");

        private void Start()
        {
            player.OnYoyoAttack += YoyoAttackAnim;
        }

        private void YoyoAttackAnim()
        {
            anim.SetTrigger(anim_YoyoAttack);
        }

        void Update()
        {
            float tiltMul = transform.forward.z * -0.5f + 0.5f; // only tilt when facing camera
            tiltedTransform.rotation = Quaternion.Euler(tiltAmount * tiltMul, 0, 0) * tiltedTransform.parent.rotation;
            if (anim)
            {
                if (dampTime > 0)
                {
                    anim.SetFloat(anim_moveSpeed, mover.MoveInput.magnitude, dampTime, Time.deltaTime);
                }
                else
                {
                    anim.SetFloat(anim_moveSpeed, mover.MoveInput.magnitude);
                }
            }
        }
    }
}