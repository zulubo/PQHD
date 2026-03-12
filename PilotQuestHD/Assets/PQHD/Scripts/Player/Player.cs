using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PQHD
{
    public class Player : MonoBehaviour
    {
        public static Player I;

        public Transform pickupMagnetPos;

        private InputAction action_attack;
        private InputAction action_interact;

        [SerializeField] private PlayerMover mover;

        public BoolTimer busy;

        [SerializeField] private Yoyo yoyoDefault;

        public Action OnYoyoAttack;


        private void Start()
        {
            action_attack = InputSystem.actions.FindAction("A");
            action_interact = InputSystem.actions.FindAction("B");
            I = this;
        }

        private void Update()
        {
            if(Dialog.DialogManager.IsPlayingDialog) return;
            
            if (action_attack.WasPressedThisFrame() && !busy)
            {
                YoyoAttack(yoyoDefault);
            }
        }


        void YoyoAttack(Yoyo yoyo)
        {
            if (yoyo.Attacking) return;
            busy.Set(yoyo.AttackDuration);
            mover.StopMoving.Set(yoyo.AttackDuration);
            yoyo.Attack();
            OnYoyoAttack?.Invoke();
        }
    }
}