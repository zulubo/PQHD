using System;
using System.Globalization;
using PQHD.Dialog;
using UnityEngine;
using UnityEngine.Audio;

namespace PQHD
{
    public class Parvina : MonoBehaviour, IInteractable
    {
        [SerializeField] private GameObject shackBroken;
        [SerializeField] private GameObject shackRebuilt;
        [SerializeField] private HitReaction buildBounce;
        [SerializeField] private AudioResource buildSound;
        [SerializeField] private DialogGraphRuntime dialog;
        [SerializeField] private Loot moonIngot;
        [SerializeField] private Loot moonDrop;
        [SerializeField] private Loot meat;

        private bool isShackBuilt;
        
        [SerializeField] GameObject[] meatSupply;
        private int meatCount;
        [SerializeField] float meatRestockTime = 240;
        private float meatRestockTimer = 0;
        private int maxMeat => meatSupply.Length;
        [SerializeField] private int meatPrice = 500;

        
        [System.Serializable]
        public class SerializedParvina
        {
            public bool shackBuilt;
            public int meatCount;
            public float meatRestockTimer;
        }

        public SerializedParvina Serialize()
        {
            return new SerializedParvina()
            {
                shackBuilt = this.isShackBuilt,
                meatCount = this.meatCount,
                meatRestockTimer = this.meatRestockTimer
            };
        }

        public void Deserialize(SerializedParvina parvina)
        {
            isShackBuilt = false;
            meatCount = 0;
            meatRestockTimer = 0;
            if (parvina != null)
            {
                isShackBuilt = parvina.shackBuilt;
                meatCount = parvina.meatCount;
                meatRestockTimer = parvina.meatRestockTimer;
            }
            UpdateShack();
            UpdateMeatDisplay();
        }

        private void UpdateShack()
        {
            shackRebuilt.SetActive(isShackBuilt);
            shackBroken.SetActive(!isShackBuilt);
        }

        private void UpdateMeatDisplay()
        {
            for(int m = 0; m < meatSupply.Length; m++)
            {
                meatSupply[m].SetActive(meatCount > m);
            }
        }

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

            if(isShackBuilt && meatCount < maxMeat)
            {
                meatRestockTimer += Time.deltaTime;
                if(meatRestockTimer > meatRestockTime)
                {
                    meatRestockTimer = 0;
                    meatCount++;
                    UpdateMeatDisplay();
                }
            }
        }

        public Vector3 GetInteractPos() => transform.position;

        private void UpdateDialogProperties()
        {
            dialog.SetProperty("IsShackBuilt", isShackBuilt.ToString(CultureInfo.InvariantCulture));
            dialog.SetProperty("HasMoonIngot", (Inventory.I.GetCount(moonIngot) > 0).ToString(CultureInfo.InvariantCulture));
            dialog.SetProperty("HasMeat", (meatCount > 0).ToString(CultureInfo.InvariantCulture));
            int moreDropsNeeded = meatPrice - Inventory.I.GetCount(moonDrop);
            dialog.SetProperty("CanAffordMeat", (moreDropsNeeded <= 0).ToString(CultureInfo.InvariantCulture));
            dialog.SetProperty("MoreDropsNeeded", moreDropsNeeded.ToString(CultureInfo.InvariantCulture));
            dialog.SetProperty("MeatAtCapacity", (Inventory.I.GetCount(meat) >= Inventory.I.GetCapacity(meat)).ToString(CultureInfo.InvariantCulture));
        }

        public void RebuildShack()
        {
            Inventory.I.Remove(moonIngot);
            isShackBuilt = true;
            UpdateShack();
            buildBounce.Bump(30);
            Audio.I.PlaySound3D(buildSound, transform.position, 30);
        }

        public void SellMeat()
        {
            if (meatCount <= 0) return;
            if (Inventory.I.GetCount(moonDrop) < meatPrice) return;
            Inventory.I.Remove(moonDrop, meatPrice);
            Inventory.I.Add(meat);
            meatCount--;
            UpdateMeatDisplay();
        }

        public void GiveFreeMeat()
        {
            Inventory.I.Add(meat);
        }
    }
}
