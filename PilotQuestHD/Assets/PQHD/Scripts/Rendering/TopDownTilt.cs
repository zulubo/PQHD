using System;
using UnityEngine;

namespace PQHD
{
    public class TopDownTilt : MonoBehaviour
    {
        [SerializeField] private float tiltWhenFacingCamera = 20;
        [SerializeField] private float tiltWhenFacingAwayFromCamera = 20;
        [SerializeField] private Transform tiltTransform;

        [Range(0,1)]
        public float tiltMultiplier = 1;
        private void Update()
        {
            float facing = transform.forward.z * -0.5f + 0.5f; // only tilt when facing camera
            tiltTransform.rotation = Quaternion.Euler(Mathf.Lerp(tiltWhenFacingAwayFromCamera, tiltWhenFacingCamera, facing) * tiltMultiplier, 0, 0) * tiltTransform.parent.rotation;
        }
    }
}
