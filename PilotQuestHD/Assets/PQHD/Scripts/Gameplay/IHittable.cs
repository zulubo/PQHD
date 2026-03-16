using UnityEngine;

namespace PQHD
{
    public interface IHittable
    {
        public void Hit(HitInfo info);

        /// <summary>
        /// Hit all hittable components on collider and its rigidbody
        /// </summary>
        public static void HitCollider(Collider collider, HitInfo info)
        {
            if (collider.attachedRigidbody && collider.attachedRigidbody.gameObject != collider.gameObject)
            {
                HitGameObject(collider.attachedRigidbody.gameObject, info);
            }
            HitGameObject(collider.gameObject, info);
            
        }

        /// <summary>
        /// Hit all hittable components on a gameobject
        /// </summary>
        /// <param name="gameObject"></param>
        public static void HitGameObject(GameObject gameObject, HitInfo info)
        {
            var hittables = gameObject.GetComponents<IHittable>();
            for (int h = 0; h < hittables.Length; h++)
            {
                hittables[h].Hit(info);
            }
        }
    }

    public enum HitType
    {
        Melee,
        Projectile,
    }

    public struct HitInfo
    {
        public int strength;
        public HitType type;
        public Vector3 direction;

        public HitInfo(int strength, Vector3 direction, HitType type)
        {
            this.strength = strength;
            this.type = type;
            this.direction = direction;
        }
    }
}