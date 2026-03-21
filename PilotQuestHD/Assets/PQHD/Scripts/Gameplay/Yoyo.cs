using System;
using UnityEngine;
using UnityEngine.Audio;

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
        [SerializeField] private AudioResource attackSound;

        [SerializeField] private float hitHapticsAmp = 0.8f;
        [SerializeField] private float hitHapticsDuration = 0.2f;

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
            Audio.I.PlaySound3D(attackSound, transform.position, 20);
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
                IHittable.HitCollider(overlapBuffer[i], new HitInfo(hitDamage, transform.forward, HitType.Melee), out bool hitAnything);
                
                if(hitAnything && hitHapticsAmp > 0)
                {
                    Input.Haptics(hitHapticsDuration, hitHapticsAmp, Input.HapticFreq.Low);
                    Input.Haptics(hitHapticsDuration * 0.5f, hitHapticsAmp, Input.HapticFreq.High);
                }
            }
        }
    }
}