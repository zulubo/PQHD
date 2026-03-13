using System;
using System.Globalization;
using PQHD.Dialog;
using UnityEngine;

namespace PQHD
{
    public class GummaTree : MonoBehaviour, IInteractable
    {
        [SerializeField] private DialogGraphRuntime dialog;
        [SerializeField] private Loot moonDrop;

        [SerializeField] private int[] seedPrices;
        private int plantCount;
        private int MaxPlants => seedPrices.Length;
        
        public Vector3 GetInteractPos() => transform.position;
        public void Interact()
        {
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
            bool seedAvailable = plantCount < MaxPlants;
            dialog.SetProperty("SeedAvailable", seedAvailable.ToString(CultureInfo.InvariantCulture));
            
            if (seedAvailable)
            {
                int price = seedPrices[plantCount];
                dialog.SetProperty("Price", price.ToString(CultureInfo.InvariantCulture));
                int moreNeeded = price - Inventory.I.GetCount(moonDrop);
                dialog.SetProperty("CanAfford", (moreNeeded <= 0).ToString(CultureInfo.InvariantCulture));
                dialog.SetProperty("MoonDropsRequired", moreNeeded.ToString(CultureInfo.InvariantCulture));
            }
        }

        public void DropSeed()
        {
            if (plantCount >= MaxPlants) return; // max plants

            Inventory.I.Remove(moonDrop, seedPrices[plantCount]);
            plantCount++;
            
            // TODO: spawn seed, plant
        }
    }
}
