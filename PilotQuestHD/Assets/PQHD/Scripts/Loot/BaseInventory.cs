using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PQHD
{
    public class BaseInventory : MonoBehaviour
    {
        [SerializeField] private Inventory inventory;
        
        [SerializeField] private float autosaveTime = 10;

        private void Start()
        {
            Saving.LoadFromDisk();
            Saving.OnLoadedState += LoadedState;
            
            StartCoroutine(AutosaveCoroutine());
        }

        private void OnDestroy()
        {
            Saving.SaveToDisk();
            Saving.OnLoadedState -= LoadedState;
        }

        private void LoadedState(Saving.SaveState state)
        {
            inventory.Deserialize(state.inventory);
        }

        IEnumerator AutosaveCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(autosaveTime);
                Saving.SaveToDisk();
            }
        }
    }
}
