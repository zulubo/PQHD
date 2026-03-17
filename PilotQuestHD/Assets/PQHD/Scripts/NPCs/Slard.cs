using UnityEngine;
using PQHD.Dialog;
using System.Globalization;

namespace PQHD
{
    public class Slard : MonoBehaviour, IHittable, IInteractable
    {
        [SerializeField] DialogGraphRuntime hitDialog;
        [SerializeField] DialogGraphRuntime interactDialog;
        [SerializeField] Loot meat;

        public void Hit(HitInfo info)
        {
            hitDialog.Play();
        }

        public void Interact()
        {
            UpdateDialogProperties();
            interactDialog.Play();
        }

        void Update()
        {
            if(interactDialog.isPlaying)
            {
                UpdateDialogProperties();
            }
        }

        void UpdateDialogProperties()
        {
            interactDialog.SetProperty("HasMeat", (Inventory.I.GetCount(meat) > 0).ToString(CultureInfo.InvariantCulture));
        }

        public Vector3 GetInteractPos() => transform.position;
    }
}
