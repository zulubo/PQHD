using UnityEngine;

namespace PQHD
{
    public interface IHittable
    {
        public void Hit();

        public static bool TryFind(Collider collider, out IHittable hittable)
        {
            if (collider.attachedRigidbody && collider.attachedRigidbody.TryGetComponent(out hittable)) return true;
            if (collider.TryGetComponent(out hittable)) return true;
            return false;
        }
    }
}