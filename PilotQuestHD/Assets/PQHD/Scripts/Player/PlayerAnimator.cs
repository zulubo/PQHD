using System;
using UnityEngine;

namespace PQHD
{
    public class PlayerAnimator : MonoBehaviour
    {
        [SerializeField] private Player player;
        [SerializeField] private PlayerMover mover;
        [SerializeField] private Animator anim;
        [SerializeField] private float dampTime = 0.1f;
        [SerializeField] private TopDownTilt tilt;

        private readonly int anim_moveSpeed = Animator.StringToHash("MoveSpeed");
        private static readonly int anim_YoyoAttack = Animator.StringToHash("YoyoAttack");
        private static readonly int anim_TiltReduction = Animator.StringToHash("TiltReduction");

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

                if(tilt) tilt.tiltMultiplier = 1 - anim.GetFloat(anim_TiltReduction);
            }
        }
    }
}