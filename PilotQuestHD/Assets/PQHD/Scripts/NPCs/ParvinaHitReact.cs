using PQHD.Dialog;
using UnityEngine;

namespace PQHD
{
    public class ParvinaHitReact : MonoBehaviour, IHittable
    {
        [SerializeField] private DialogGraphRuntime dialog;
        public void Hit(HitInfo info)
        {
            dialog.Play();
        }
    }
}
