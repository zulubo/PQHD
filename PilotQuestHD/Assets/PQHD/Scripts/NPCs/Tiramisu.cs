using System;
using UnityEngine;
using PQHD.Dialog;
using System.Collections;
using UnityEditor.Analytics;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace PQHD
{
    public class Tiramisu : MonoBehaviour, IHittable, IInteractable
    {
        [SerializeField] NavMeshAgent nav;
        [SerializeField] private Rigidbody rb;
        [SerializeField] Vector2 moveDistanceMinMax;
        [SerializeField] Vector2 moveTimeMinMax;

        [SerializeField] Transform crystal;
        [SerializeField] float crystalAvoidRadius = 8;

        [SerializeField] float playerStandStillRadius = 6;

        [SerializeField] DialogGraphRuntime hitDialog;
        [SerializeField] DialogGraphRuntime interactDialog;

        [SerializeField] private TopDownTilt tilt;

        [SerializeField] private Animator anim;
        private readonly int anim_walkSpeed = Animator.StringToHash("WalkSpeed");
        private readonly int anim_fallOver = Animator.StringToHash("FallOver");
        private readonly int anim_walkState = Animator.StringToHash("Walk");
        private readonly int anim_tiltReduction = Animator.StringToHash("TiltReduction");

        [SerializeField] private float turnSpeed = 1000;
        private Vector3 facingDir;

        private void Start()
        {
            // moved with rigidbody in fixedupdate
            nav.updatePosition = false;
            facingDir = transform.forward;
        }

        void OnEnable()
        {
            StartCoroutine(MoveCoroutine());
        }

        public Vector3 GetInteractPos() => transform.position;

        IEnumerator MoveCoroutine()
        {
            while(true)
            {
                yield return new WaitForSeconds(Random.Range(moveTimeMinMax.x, moveTimeMinMax.y));

                // stand still if near player
                if(Player.I && Vector3.Distance(Player.I.transform.position, transform.position) < playerStandStillRadius)
                {
                    continue;
                }

                if (!Walking) continue;

                Move();
            }
        }

        void Move()
        {
            // find a good direction to move
            float bestWeight = Mathf.NegativeInfinity;
            Vector3 bestMove = transform.position;
            int dir = 0;
            for(int r = 0; r < 4; r++)
            {
                Vector3 moveVec = Quaternion.Euler(0, r*90, 0) * Vector3.forward * Random.Range(moveDistanceMinMax.x, moveDistanceMinMax.y);
                Vector3 movePos = transform.position + moveVec;
                float weight = 1;

                // avoid crystal
                if(crystal)
                {
                    Vector3 closestToCrystal = Math3d.ProjectPointOnLineSegment(transform.position, movePos, crystal.position);
                    if(Vector3.Distance(closestToCrystal, crystal.transform.position) < crystalAvoidRadius)
                    {
                        weight -= 100;
                    }
                }

                // don't walk into walls
                if(NavMesh.Raycast(transform.position, movePos, out NavMeshHit hit, NavMesh.AllAreas))
                {
                    weight -= moveVec.magnitude - hit.distance;
                }

                weight += Random.value * 0.25f;

                if(weight > bestWeight)
                {
                    bestWeight = weight;
                    bestMove = movePos;
                }
            }

            nav.SetDestination(bestMove);
        }
        
        private bool Walking => anim.GetCurrentAnimatorStateInfo(0).shortNameHash == anim_walkState;

        private void Update()
        {
            anim.SetFloat(anim_walkSpeed, nav.velocity.magnitude / nav.speed, 0.1f, Time.deltaTime);

            nav.enabled = Walking;
            if (nav.desiredVelocity.sqrMagnitude > 0.1f)
            {
                facingDir = nav.desiredVelocity.normalized;
            }
            
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(facingDir), turnSpeed * Time.deltaTime);

            tilt.tiltMultiplier = 1 - anim.GetFloat(anim_tiltReduction);
        }

        private void FixedUpdate()
        {
            rb.MovePosition(nav.nextPosition);
        }

        public void Hit(HitInfo info)
        {
            hitDialog.Play();
            if(Walking) anim.SetTrigger(anim_fallOver);
        }

        public void Interact()
        {
            interactDialog.Play();
        }
    }
}
