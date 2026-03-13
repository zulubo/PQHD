using PQHD.Dialog;
using UnityEngine;

namespace PQHD
{
    public class SimpleInteractDialog : MonoBehaviour, IInteractable
    {
        [SerializeField] DialogGraphRuntime dialog;

        public void Interact()
        {
            dialog.Play();
        }

        public Vector3 GetInteractPos() => transform.position;
    }
}
