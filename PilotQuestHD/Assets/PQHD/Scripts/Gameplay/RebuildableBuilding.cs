using UnityEngine;
using PQHD.Dialog;
using UnityEngine.Audio;
using System.Globalization;

namespace PQHD
{
    public class RebuildableBuilding : MonoBehaviour, IInteractable
    {
        [SerializeField] private GameObject brokenObject;
        [SerializeField] private GameObject rebuiltObject;
        [SerializeField] private HitReaction buildBounce;
        [SerializeField] private AudioResource buildSound;
        [SerializeField] private DialogGraphRuntime dialog;
        [SerializeField] private Loot costItem;
        [SerializeField] private int costAmount;
        private bool isRebuilt;

        public Vector3 GetInteractPos() => transform.position;

        public void Interact()
        {
            UpdateDialogProperties();
            dialog.Play();
        }

        private void UpdateDialogProperties()
        {
            int moreLootNeeded = costAmount - Inventory.I.GetCount(costItem);
            dialog.SetProperty("CanAfford", (moreLootNeeded <= 0).ToString(CultureInfo.InvariantCulture));
            dialog.SetProperty("MoreLootNeeded", moreLootNeeded.ToString(CultureInfo.InvariantCulture));
        }

        public void Rebuild()
        {
            Inventory.I.Remove(costItem, costAmount);
            isRebuilt = true;
            UpdateObjects();
            buildBounce.Bump(30);
            Audio.I.PlaySound3D(buildSound, transform.position, 30);
        }

        private void UpdateObjects()
        {
            rebuiltObject.SetActive(isRebuilt);
            brokenObject.SetActive(!isRebuilt);
        }
    }
}
