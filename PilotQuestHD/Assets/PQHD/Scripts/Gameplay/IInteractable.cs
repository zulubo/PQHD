using System.Runtime.InteropServices;
using UnityEngine;

namespace PQHD
{
    public interface IInteractable
    {
        public void Interact();
        public Vector3 GetInteractPos();

        /// <summary>
        /// Find an interactable on a collider or its attached rigidbody
        /// </summary>
        public static bool GetFromCollider(Collider col, out IInteractable interactable)
        {
            if(col.attachedRigidbody && col.attachedRigidbody.TryGetComponent(out interactable)) return true;
            if(col.TryGetComponent(out interactable)) return true;
            return false; 
        }
    }
}
