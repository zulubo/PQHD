using System;
using UnityEngine;

namespace PQHD
{
    public class CameraFollow : MonoBehaviour
    {
        public Transform target;

        [SerializeField] private Vector2 deadZone;
        [SerializeField] private float horizontalSmoothing;
        [SerializeField] private float verticalSmoothing;

        private void Update()
        {
            Vector3 followPos = target.position;
            Vector3 followOffset = followPos - transform.position;
            Vector3 followOffsetWithDeadzone = followOffset;
            followOffsetWithDeadzone.x = followOffset.x - Mathf.Clamp(followOffset.x, -deadZone.x, deadZone.x);
            followOffsetWithDeadzone.z = followOffset.z - Mathf.Clamp(followOffset.z, -deadZone.y, deadZone.y);

            followPos = transform.position + followOffsetWithDeadzone;

            if (CameraLimits.Exists)
            {
                CameraLimits.Active.ClampPosition(ref followPos);
            }

            Vector3 pos = transform.position;

            if (horizontalSmoothing > 0)
            {
                pos.x = Mathf.Lerp(pos.x, followPos.x, Time.deltaTime / horizontalSmoothing);
                pos.z = Mathf.Lerp(pos.z, followPos.z, Time.deltaTime / horizontalSmoothing);
            }
            else
            {
                pos.x = followPos.x;
                pos.z = followPos.z;
            }

            if (verticalSmoothing > 0)
            {
                pos.y = Mathf.Lerp(pos.y, followPos.y, Time.deltaTime / horizontalSmoothing);
            }
            else
            {
                pos.y = followPos.y;
            }

            transform.position = pos;
        }
    }
}