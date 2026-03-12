using System;
using System.Collections.Generic;
using UnityEngine;

namespace PQHD
{
    public class CameraLimits : MonoBehaviour
    {
        public static List<CameraLimits> All = new();
        public static bool Exists => All.Count > 0;
        public static CameraLimits Active => All[0]; // todo: some sorting?


        [SerializeField] private Vector3 size;

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireCube(transform.position, size);
        }

        private void OnEnable()
        {
            All.Add(this);
        }

        private void OnDisable()
        {
            All.Remove(this);
        }

        public void ClampPosition(ref Vector3 pos)
        {
            pos -= transform.position;

            pos.x = Mathf.Clamp(pos.x, size.x * -0.5f, size.x * 0.5f);
            pos.y = Mathf.Clamp(pos.y, size.y * -0.5f, size.y * 0.5f);
            pos.z = Mathf.Clamp(pos.z, size.z * -0.5f, size.z * 0.5f);

            pos += transform.position;
        }
    }
}