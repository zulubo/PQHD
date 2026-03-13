using UnityEngine;
using PQHD.Dialog;

namespace PQHD
{
    public class Slard : MonoBehaviour, IHittable
    {
        [SerializeField] DialogGraphRuntime hitDialog;
        [SerializeField] DialogGraphRuntime interactDialog;

        public void Hit(HitInfo info)
        {
            hitDialog.Play();
        }
    }
}
