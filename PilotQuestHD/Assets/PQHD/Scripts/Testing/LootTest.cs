using UnityEditor.Overlays;
using UnityEngine;

namespace PQHD
{
    public class LootTest : MonoBehaviour
    {
        [SerializeField] Loot moonDrop;
        [SerializeField] Loot moonIngot;

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
