using System;
using System.Globalization;
using PQHD.Dialog;
using UnityEngine;

namespace PQHD
{
    public class Parvina : MonoBehaviour, IInteractable
    {
        [SerializeField] private GameObject shackBroken;
        [SerializeField] private GameObject shackRebuilt;

        [SerializeField] private DialogGraphRuntime dialog;

        [SerializeField] private Loot moonIngot;
        [SerializeField] private Loot meat;

        private bool isShackBuilt;
        
        private int meatCount;
        
        [System.Serializable]
        public class SerializedParvina
        {
            public bool shackBuilt;
            public int meatCount;
        }

        public SerializedParvina Serialize()
        {
            return new SerializedParvina()
            {
                shackBuilt = this.isShackBuilt,
                meatCount = this.meatCount
            };
        }

        public void Deserialize(SerializedParvina parvina)
        {
            isShackBuilt = false;
            meatCount = 0;
            if (parvina != null)
            {
                isShackBuilt = parvina.shackBuilt;
                meatCount = parvina.meatCount;
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
        }

        public Vector3 GetInteractPos() => transform.position;

        private void UpdateDialogProperties()
        {
            dialog.SetProperty("IsShackBuilt", isShackBuilt.ToString(CultureInfo.InvariantCulture));
            dialog.SetProperty("HasMoonIngot", (Inventory.I.GetCount(moonIngot) > 0).ToString(CultureInfo.InvariantCulture));
            dialog.SetProperty("HasMeat", (meatCount > 0).ToString(CultureInfo.InvariantCulture));
        }

        public void RebuildShack()
        {
            Inventory.I.Remove(moonIngot);
            isShackBuilt = true;
            UpdateShack();
        }

        public void SellMeat()
        {
            if (meatCount <= 0) return;
            if (Inventory.I.GetCount(moonIngot) == 0) return;
            Inventory.I.Remove(moonIngot);
            Inventory.I.Add(meat);
            meatCount--;
            if (meatCount < 0) meatCount = 0;
            UpdateMeatDisplay();
        }

        public void GiveFreeMeat()
        {
            Inventory.I.Add(meat);
        }
    }
}
