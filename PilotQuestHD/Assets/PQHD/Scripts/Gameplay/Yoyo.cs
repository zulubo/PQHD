using System;
using UnityEngine;

namespace PQHD
{
    public class Yoyo : MonoBehaviour
    {
        [SerializeField] private GameObject yoyoVisual;
        [SerializeField] private Transform handPos;

        [SerializeField] private Vector3 attackPos;
        [SerializeField] private float attackRadius;
        [SerializeField] private LayerMask attackMask;
        [SerializeField] private float attackDuration = 0.3f;
        public float AttackDuration => attackDuration;
        [SerializeField] private float hitDelay = 0.1f;

        private bool attacking;
        public bool Attacking => attacking;
        private bool hasHit;
        private float attackTimer;

        [SerializeField] private int hitDamage = 1;


        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.TransformPoint(attackPos), attackRadius);
        }

        private void Start()
        {
            yoyoVisual.SetActive(false);
        }

        public void Attack()
        {
            if (attacking) return;
            yoyoVisual.SetActive(true);
            attacking = true;
            hasHit = false;
            attackTimer = 0;
        }

        private void Update()
        {
            if (attacking)
            {
                if (handPos) yoyoVisual.transform.position = handPos.position;

                attackTimer += Time.deltaTime;
                if (attackTimer > hitDelay && !hasHit)
                {
                    Hit();
                    hasHit = true;
                }

                if (attackTimer > attackDuration)
                {
                    yoyoVisual.SetActive(false);
                    attacking = false;
                }
            }
        }

        private readonly Collider[] overlapBuffer = new Collider[64];

        private void Hit()
        {
            int overlapCount = Physics.OverlapSphereNonAlloc(transform.TransformPoint(attackPos), attackRadius,
                overlapBuffer, attackMask);

            for (int i = 0; i < overlapCount; i++)
            {
                IHittable.HitCollider(overlapBuffer[i], new HitInfo(hitDamage, HitType.Melee));
            }
        }
    }
}