using System;
using UnityEngine;

namespace PQHD
{
    public class MagnetPickup : MonoBehaviour
    {
        [SerializeField] private float magnetRadius;
        private const float MagnetSpeed = 20;
        private new Rigidbody rigidbody;

        [SerializeField] private float magnetDelay = 1f;
        [SerializeField] private float magnetDelayEaseIn = 1f;
        [SerializeField] private float despawnTime = 15;
        private const float FlickerDuration = 2f;
        private const float FlickerPeriod = 0.08f;
        [SerializeField] private GameObject flickerObject;
        private float spawnTime;
        private float flickerTimer;

        [SerializeField] private Loot loot;
        

        private void Start()
        {
            rigidbody = GetComponent<Rigidbody>();
            spawnTime = Time.timeSinceLevelLoad;
        }

        private const float pickupRadius = 0.2f;

        private void FixedUpdate()
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
                    rigidbody.MovePosition(Vector3.MoveTowards(rigidbody.position, Player.I.pickupMagnetPos.position,
                        magnetStrength * MagnetSpeed * Time.fixedDeltaTime));
                }

                if (dist < pickupRadius)
                {
                    PickedUp();
                }
            }
        }

        private void Update()
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