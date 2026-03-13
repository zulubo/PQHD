using UnityEngine;
using PQHD.Dialog;

namespace PQHD
{
    public class Slard : MonoBehaviour, IHittable, IInteractable
    {
        [SerializeField] DialogGraphRuntime hitDialog;
        [SerializeField] DialogGraphRuntime interactDialog;

        public void Hit(HitInfo info)
        {
            hitDialog.Play();
        }

        public void Interact()
        {
            interactDialog.Play();
        }

        public Vector3 GetInteractPos() => transform.position;
    }
}
