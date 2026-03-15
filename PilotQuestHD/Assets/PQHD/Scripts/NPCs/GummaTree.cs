using System;
using System.Collections;
using System.Globalization;
using PQHD.Dialog;
using UnityEngine;
using UnityEngine.Audio;

namespace PQHD
{
    public class GummaTree : MonoBehaviour, IInteractable
    {
        [SerializeField] private DialogGraphRuntime dialog;
        [SerializeField] private Loot moonDrop;
        [SerializeField] private Farm farm;
        [SerializeField] private GameObject seedPrefab;
        [SerializeField] private AudioResource seedSpawnSound;
        [SerializeField] private Transform seedSpawnPos;
        [SerializeField] private GameObject seedImpactFX;

        [SerializeField] private float seedArcDuration = 1f;
        [SerializeField] private float seedArcHeight = 1f;
        
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

        private bool spawningSeed;

        public void DropSeed()
        {
            if(spawningSeed) return;
            Farm.FarmPlant nextPlant = farm.GetNextPlant();
            if(nextPlant == null) return;

            Inventory.I.Remove(moonDrop, nextPlant.price);
            
            nextPlant.SetGrownWithoutActivate(true);
            StartCoroutine(SpawnPlantCoroutine(nextPlant.plant));
        }

        IEnumerator SpawnPlantCoroutine(Plant plant)
        {
            spawningSeed = true;
            GameObject seed = Instantiate(seedPrefab, seedSpawnPos.position, Quaternion.identity);
            // TODO: play sound
            float t = 0;
            while(t < 1)
            {
                t += Time.deltaTime / seedArcDuration;
                Vector3 arcOffset = seedArcHeight * (1 - 4 * (t - 0.5f) * (t - 0.5f)) * Vector3.up;
                seed.transform.position = Vector3.Lerp(seedSpawnPos.position, plant.transform.position, t) + arcOffset;
                yield return null;
            }
            Destroy(seed);
            if(seedImpactFX) Instantiate(seedImpactFX, plant.transform.position, Quaternion.identity);
            plant.Grow();
            spawningSeed = false;
        }
    }
}
