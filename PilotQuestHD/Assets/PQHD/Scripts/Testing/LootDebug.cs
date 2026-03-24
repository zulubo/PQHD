using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PQHD
{
    public class LootDebug : MonoBehaviour
    {
        [SerializeField] Loot moonDrop;
        [SerializeField] Loot moonIngot;

        [SerializeField] private GameObject root;

        private void Update()
        {
            if (Keyboard.current.gKey.wasPressedThisFrame 
             && Keyboard.current.dKey.isPressed
             && Keyboard.current.eKey.isPressed
             && Keyboard.current.bKey.isPressed
             && Keyboard.current.uKey.isPressed)
            {
                root.gameObject.SetActive(!root.gameObject.activeSelf);
            }
        }

        public void GiveDrops(int count)
        {
            Inventory.I.Add(moonDrop, count);            
        }

        public void GiveIngots(int count)
        {
            Inventory.I.Add(moonIngot, count);
        }

        public void ResetInventory()
        {
            Inventory.I.RemoveAll();
        }

        public void ResetSave()
        {
            Saving.DeleteSave();
            Saving.LoadFromDisk();
        }

    }
}
