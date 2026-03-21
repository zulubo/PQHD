using System;
using UnityEngine;

namespace PQHD
{
    public class Player : MonoBehaviour
    {
        public static Player I;

        public Transform pickupMagnetPos;

        [SerializeField] private PlayerMover mover;

        [SerializeField] private Vector3 interactPos;
        [SerializeField] private float interactRadius;
        [SerializeField] private LayerMask interactMask;

        public BoolTimer busy;

        [SerializeField] private Yoyo yoyoDefault;
        [System.Serializable]
        private class AltYoyo
        {
            public Yoyo yoyo;
            public Loot item;
        }
        [SerializeField] AltYoyo[] altYoyos;

        public Action OnYoyoAttack;


        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.TransformPoint(interactPos), interactRadius);
        }

        void Awake()
        {
            I = this;
        }


        private void Update()
        {
            if(Dialog.DialogManager.IsPlayingDialog) busy.Set(0.1f);
            if(StoreRuntime.active) busy.Set(0.1f);
            
            if (Input.ButtonB.WasPressedThisFrame && !busy)
            {
                YoyoAttack(GetEquippedYoyo());
            }

            if(Input.ButtonA.WasPressedThisFrame && !busy)
            {
                Interact();
            }
        }

        Yoyo GetEquippedYoyo()
        {
            for(int i = altYoyos.Length - 1; i >= 0; i--)
            {
                if(Inventory.I.GetCount(altYoyos[i].item) > 0)
                {
                    return altYoyos[i].yoyo;
                }
            }
            return yoyoDefault;
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