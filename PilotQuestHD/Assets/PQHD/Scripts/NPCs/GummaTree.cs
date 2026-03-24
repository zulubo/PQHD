using System;
using System.Collections;
using System.Globalization;
using PQHD.Dialog;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;

namespace PQHD
{
    public class GummaTree : MonoBehaviour, IInteractable
    {
        [SerializeField] private DialogGraphRuntime dialog;
        [SerializeField] private Loot moonDrop;
        [SerializeField] private Farm farm;
        [SerializeField] private GameObject seedPrefab;
        [SerializeField] private AudioResource seedSpawnSound;
        [SerializeField] private AudioResource seedPlantSound;
        [SerializeField] private Transform seedSpawnPos;
        [SerializeField] private GameObject seedImpactFX;

        [SerializeField] private float seedArcDuration = 1f;
        [SerializeField] private float seedArcHeight = 1f;

        [SerializeField] private Transform eyeTransform;
        private Quaternion eyeRestRot;
        [SerializeField] private float lookAtPlayerRadius = 10;
        [SerializeField] private Vector2 lookAmount = new Vector2(0.6f, 0.6f);
        [SerializeField] private Vector2 maxLookAngle = new Vector2(30, 30);
        [SerializeField] private float eyeMoveSpeed = 8;

        private void Start()
        {
            eyeRestRot = eyeTransform.rotation;
        }

        public Vector3 GetInteractPos() => transform.position;
        public void Interact()
        {
            if(spawningSeed) return;
            UpdateDialogProperties();
            dialog.Play();
        }


        private void Update()
        {
            if (dialog.isPlaying)
            {
                UpdateDialogProperties();
            }
            
            UpdateEye();
        }

        void UpdateDialogProperties()
        {
            Farm.FarmPlant nextPlant = farm.GetNextPlant();
            bool seedAvailable = nextPlant != null;
            dialog.SetProperty("SeedAvailable", seedAvailable.ToString(CultureInfo.InvariantCulture));
            
            if (seedAvailable)
            {
                int price = nextPlant.price;
                dialog.SetProperty("Price", price.ToString(CultureInfo.InvariantCulture));
                int moreNeeded = price - Inventory.I.GetCount(moonDrop);
                dialog.SetProperty("CanAfford", (moreNeeded <= 0).ToString(CultureInfo.InvariantCulture));
                dialog.SetProperty("MoonDropsRequired", moreNeeded.ToString(CultureInfo.InvariantCulture));
            }
        }

        private void UpdateEye()
        {
            Quaternion targetEyeRot = eyeRestRot;
            if (Player.I && (Player.I.transform.position - transform.position).sqrMagnitude <
                lookAtPlayerRadius * lookAtPlayerRadius)
            {
                Vector3 dir = (Player.I.transform.position - eyeTransform.position).normalized;
                Quaternion lookRot = Quaternion.FromToRotation(transform.forward, dir);
                lookRot.ToAngleAxis(out float lookAngle, out Vector3 lookAxis);
                float vertical = Mathf.Abs(dir.y);
                lookAngle *= Mathf.LerpAngle(lookAmount.x, lookAmount.y, vertical);
                float maxAngle = Mathf.LerpAngle(maxLookAngle.x, maxLookAngle.y, vertical);
                lookAngle = Mathf.Clamp(lookAngle, -maxAngle, maxAngle);
                lookRot = Quaternion.AngleAxis(lookAngle, lookAxis);
                targetEyeRot = lookRot * eyeRestRot;
            }
            
            eyeTransform.rotation = Quaternion.Slerp(eyeTransform.rotation, targetEyeRot, Time.deltaTime * eyeMoveSpeed);
        }

        private bool spawningSeed;

        public void DropSeed()
        {
            if(spawningSeed) return;
            Farm.FarmPlant nextPlant = farm.GetNextPlant();
            if(nextPlant == null) return;

            Inventory.I.Remove(moonDrop, nextPlant.price);
            
            nextPlant.SetGrownWithoutActivate(true);
            Audio.I.PlaySound2D(seedSpawnSound);
            StartCoroutine(SpawnPlantCoroutine(nextPlant.plant));
        }

        IEnumerator SpawnPlantCoroutine(Plant plant)
        {
            spawningSeed = true;
            GameObject seed = Instantiate(seedPrefab, seedSpawnPos.position, Quaternion.identity);
            float t = 0;
            while(t < 1)
            {
                t += Time.deltaTime / seedArcDuration;
                Vector3 arcOffset = seedArcHeight * (1 - 4 * (t - 0.5f) * (t - 0.5f)) * Vector3.up;
                seed.transform.position = Vector3.Lerp(seedSpawnPos.position, plant.transform.position, t) + arcOffset;
                yield return null;
            }
            Destroy(seed);
            Audio.I.PlaySound3D(seedPlantSound, plant.transform.position, 20);
            if(seedImpactFX) Instantiate(seedImpactFX, plant.transform.position, Quaternion.identity);
            plant.Grow();
            spawningSeed = false;
        }
    }
}
