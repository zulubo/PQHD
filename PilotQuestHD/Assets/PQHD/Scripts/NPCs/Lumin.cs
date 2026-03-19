using UnityEngine;
using PQHD.Dialog;

namespace PQHD
{
    public class Lumin : MonoBehaviour, IHittable, IInteractable
    {
        [SerializeField] DialogGraphRuntime interactDialog;
        [SerializeField] DialogGraphRuntime hitDialog;
        

        public Vector3 GetInteractPos() => transform.position;

        public void Hit(HitInfo info)
        {
            hitDialog.Play();
        }

        public void Interact()
        {
            interactDialog.Play();
        }
    }
}
