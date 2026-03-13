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

        [SerializeField] private Vector3 interactPos;
        [SerializeField] private float interactRadius;
        [SerializeField] private LayerMask interactMask;

        public BoolTimer busy;

        [SerializeField] private Yoyo yoyoDefault;

        public Action OnYoyoAttack;


        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.TransformPoint(interactPos), interactRadius);
        }


        private void Start()
        {
            action_attack = InputSystem.actions.FindAction("A");
            action_interact = InputSystem.actions.FindAction("B");
            I = this;
        }

        private void Update()
        {
            if(Dialog.DialogManager.IsPlayingDialog) busy.Set(0.1f);
            
            if (action_attack.WasPressedThisFrame() && !busy)
            {
                YoyoAttack(yoyoDefault);
            }

            if(action_interact.WasPressedThisFrame() && !busy)
            {
                Interact();
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
        
        private readonly Collider[] overlapBuffer = new Collider[64];
        private void Interact()
        {
            Vector3 interactPosGlobal = transform.TransformPoint(interactPos);
            int overlapCount = Physics.OverlapSphereNonAlloc(interactPosGlobal, interactRadius,
                overlapBuffer, interactMask);

            float bestDistSqr = Mathf.Infinity;
            IInteractable bestInteract = null;
            for (int i = 0; i < overlapCount; i++)
            {
                if(IInteractable.GetFromCollider(overlapBuffer[i], out IInteractable interactable))
                {
                    float distSqr = (interactable.GetInteractPos() - interactPosGlobal).sqrMagnitude;
                    if(distSqr < bestDistSqr)
                    {
                        bestDistSqr = distSqr;
                        bestInteract = interactable;
                    }
                }
            }

            if(bestInteract != null) bestInteract.Interact();
        }
    }
}