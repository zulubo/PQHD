using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PQHD
{
    public class MoonCrystal : MonoBehaviour, IHittable
    {
        [SerializeField] private GameObject moonDropPrefab;
        [SerializeField] private GameObject moonIngotPrefab;
        [SerializeField] private float moonIngotChance = 0.01f;

        [SerializeField] private Transform spawnPos;
        [SerializeField] private float spawnMinAngle = 0;
        [SerializeField] private float spawnMaxAngle = 60;
        [SerializeField] private float spawnMinSpeed = 2f;
        [SerializeField] private float spawnMaxSpeed = 4f;

        


        public void Hit(HitInfo info)
        {
            if (info.type != HitType.Melee) return;
            
            for (int d = 0; d < info.strength; d++)
            {
                SpawnSomething();
            }
        }

        private void SpawnSomething()
        {
            if (Random.value < moonIngotChance)
            {
                Spawn(moonIngotPrefab);
            }
            else
            {
                Spawn(moonDropPrefab);
            }
        }

        private void Spawn(GameObject item)
        {
            GameObject inst = Instantiate(item, spawnPos.position, Quaternion.identity);
            Rigidbody rb = inst.GetComponent<Rigidbody>();
            rb.linearVelocity = Quaternion.Euler(Random.Range(spawnMinAngle, spawnMaxAngle), Random.Range(0, 360), 0) *
                                Vector3.forward * Random.Range(spawnMinSpeed, spawnMaxSpeed);
        }

    }
}