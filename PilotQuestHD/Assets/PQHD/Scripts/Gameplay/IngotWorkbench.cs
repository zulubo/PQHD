using System.Globalization;
using PQHD.Dialog;
using UnityEngine;

namespace PQHD
{
    public class IngotWorkbench : MonoBehaviour, IInteractable
    {
        [SerializeField] private Loot moonDrop;
        [SerializeField] private Loot moonIngot;
        [SerializeField] private DialogGraphRuntime dialog;
        
        public void CreateIngot()
        {
            if (Inventory.I.GetCount(moonDrop) < 1000) return;
            
            Inventory.I.Remove(moonDrop, 1000);
            Inventory.I.Add(moonIngot);
            Audio.I.PlaySound2D(moonIngot.pickupSound);
        }

        public Vector3 GetInteractPos() => transform.position;
        public void Interact()
        {
            UpdateDialogProperties();
            dialog.Play();
        }

        void UpdateDialogProperties()
        {
            int have = Inventory.I.GetCount(moonDrop);
            dialog.SetProperty("CanAfford", (have >= 1000).ToString(CultureInfo.InvariantCulture));
            dialog.SetProperty("MoonDropsRequired", (1000 - have).ToString(CultureInfo.InvariantCulture));
        }
    }
}
