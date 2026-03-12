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

        [SerializeField] private Transform reactTransform;
        [SerializeField] private Vector2 reactSpring = new Vector2(200, 100);
        [SerializeField] private Vector2 reactDamp = new Vector2(25, 20);
        [SerializeField] private float reactAmount = 0.5f;
        private SpringDamp2 reactSim;

        private void Start()
        {
            reactSim = new SpringDamp2(Vector2.one, reactSpring, reactDamp);
        }

        public void Hit()
        {
            SpawnSomething();
            reactSim.Bump(new Vector2(reactAmount, -reactAmount));
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

        private void Update()
        {
            reactSim.UpdateSubstepped(Vector2.one, Time.deltaTime, 4);
            reactTransform.transform.localScale = new Vector3(reactSim.Position.x, reactSim.Position.y, 1);
        }
    }
}