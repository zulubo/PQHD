using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PQHD
{
    public class BaseManager : MonoBehaviour
    {
        [SerializeField] private Inventory inventory;
        [SerializeField] private Farm farm;
        [SerializeField] private float autosaveTime = 10;
        [SerializeField] private Parvina parvina;
        [SerializeField] private StoreRuntime luminStore;

        private void Start()
        {
            Saving.OnLoadedState += LoadedState;
            Saving.LoadFromDisk();
            
            StartCoroutine(AutosaveCoroutine());
        }

        private void OnDestroy()
        {
            Save();
            Saving.OnLoadedState -= LoadedState;
        }

        private void LoadedState(Saving.SaveState state)
        {
            inventory.Deserialize(state.inventory);
            farm.Deserialize(state.plants);
            parvina.Deserialize(state.parvina);
            luminStore.Deserialize(state.luminStore);
        }

        IEnumerator AutosaveCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(autosaveTime);
                Save();
            }
        }

        void Save()
        {
            Saving.State.inventory = inventory.Serialize();
            Saving.State.plants = farm.Serialize();
            Saving.State.parvina = parvina.Serialize();
            Saving.State.luminStore = luminStore.Serialize();
            Saving.SaveToDisk();
        }
    }
}
