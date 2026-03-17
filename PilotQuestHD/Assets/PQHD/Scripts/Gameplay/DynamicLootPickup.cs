using System;
using UnityEngine;

namespace PQHD
{
    /// loot that bounces around and magnets toward the player
    public class DynamicLootPickup : MonoBehaviour
    {
        [SerializeField] private Loot loot;

        [Header("Physics")] 
        [SerializeField] private float gravityMultiplier = 1.0f;
        [SerializeField] private new SphereCollider collider;
        [Range(0, 1)] [SerializeField] private float bounciness = 0.3f;
        [Range(0, 1)] [SerializeField] private float bounceFriction = 0.2f;
        [Range(0, 1)] [SerializeField] private float slidingFriction = 0.3f;
        [SerializeField] private float drag = 0.1f;
        private const float BounceThreshold = 1f;
        
        [Header("Magneting")]
        [SerializeField] private float magnetRadius;
        private const float MagnetSpeed = 20;

        [SerializeField] private float magnetDelay = 1f;
        [SerializeField] private float magnetDelayEaseIn = 1f;
        
        [Header("Despawning")]
        [SerializeField] private float despawnTime = 15;
        private const float FlickerDuration = 2f;
        private const float FlickerPeriod = 0.08f;
        [SerializeField] private GameObject flickerObject;
        private float spawnTime;
        private float flickerTimer;
        
        public Vector3 Velocity { get; set; }

        private LayerMask collisionMask;

        private void Start()
        {
            spawnTime = Time.timeSinceLevelLoad;
            collisionMask = LayerMaskExtensions.GetPhysicsLayerMask(gameObject.layer);
        }

        private const float pickupRadius = 0.2f;


        private void FixedUpdate()
        {
            
        }

        private void Update()
        {
            UpdateDespawning();
            UpdateMagneting();
            UpdatePhysics();
        }

        private static readonly Collider[] overlapBuffer = new Collider[16];
        private void UpdatePhysics()
        {
            float deltaTime = Mathf.Min(Time.deltaTime, 0.05f);

            Vector3 forces = Physics.gravity * gravityMultiplier;
            
            Velocity += forces * deltaTime;
            
            Vector3 move = Velocity * deltaTime;
            transform.position += move;
            
            int overlapCount = Physics.OverlapSphereNonAlloc(collider.transform.TransformPoint(collider.center), 
                collider.radius + 0.01f, overlapBuffer, collisionMask);
            Vector3 bounceVelocity = Vector3.zero;
            bool colliding = overlapCount > 0;
            for (int i = 0; i < overlapCount; i++)
            {
                Physics.ComputePenetration(collider, collider.transform.position, collider.transform.rotation, 
                    overlapBuffer[i], overlapBuffer[i].transform.position, overlapBuffer[i].transform.rotation, out Vector3 normal, out float penetrateDistance);

                if (penetrateDistance > 0)
                {
                    normal.Normalize();
                    
                    // add bounce velocity
                    float bounceVel = -Vector3.Dot(Velocity, normal);
                    if (bounceVel > BounceThreshold)
                    {
                        bounceVelocity += normal * (bounceVel * bounciness) * 0.5f; // idk why this needs to be halved
                    }

                    // clip velocity
                    Velocity = Vector3.ProjectOnPlane(Velocity, normal);

                    // bounce friction
                    Velocity = Vector3.Lerp(Velocity, Vector3.zero, bounceFriction);

                    // depenetrate position
                    transform.position += normal * penetrateDistance;
                }
            }

            if (colliding)
            {
                // sliding friction
                Velocity = Vector3.Lerp(Velocity, Vector3.zero, Time.deltaTime * slidingFriction);
            }

            // apply bouncing
            Velocity += bounceVelocity;
            
            // drag
            Velocity = Vector3.Lerp(Velocity, Vector3.zero, Time.deltaTime * drag);

            Velocity += bounceVelocity;
        }

        private void UpdateMagneting()
        {
            if (Player.I)
            {
                Vector3 toPlayer = Player.I.pickupMagnetPos.position - transform.position;
                float dist = toPlayer.magnitude;
                float magnetStrength = Mathf.Clamp01(1 - toPlayer.magnitude / magnetRadius);

                float age = Time.timeSinceLevelLoad - spawnTime;
                magnetStrength *= Mathf.InverseLerp(magnetDelay, magnetDelay + magnetDelayEaseIn, age);

                magnetStrength *= magnetStrength;

                if (magnetStrength > 0)
                {
                    transform.position = Vector3.MoveTowards(transform.position, Player.I.pickupMagnetPos.position, magnetStrength * MagnetSpeed * Time.deltaTime);
                }

                if (dist < pickupRadius)
                {
                    PickedUp();
                }
            }
        }

        private void UpdateDespawning()
        {
            float age = Time.timeSinceLevelLoad - spawnTime;

            if (age > despawnTime - FlickerDuration)
            {
                flickerTimer += Time.deltaTime;
                if (flickerTimer >= FlickerPeriod)
                {
                    flickerTimer = 0;
                    flickerObject.SetActive(!flickerObject.activeSelf);
                }
            }

            if (age > despawnTime) Despawn();
        }

        private bool despawned;

        void PickedUp()
        {
            if (despawned) return;

            if(loot && Inventory.I) Inventory.I.Add(loot);
            Destroy(gameObject);
            despawned = true;
        }

        void Despawn()
        {
            Destroy(gameObject);
            despawned = true;
        }
    }
}