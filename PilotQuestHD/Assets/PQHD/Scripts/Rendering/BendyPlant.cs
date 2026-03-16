using UnityEngine;

namespace PQHD
{
    public class BendyPlant : MonoBehaviour, IHittable
    {
        [SerializeField] private SoftCollider col;
        [SerializeField] private float spring = 100;
        [SerializeField] private float damp = 2f;
        [SerializeField] private float hitReaction = 2f;

        [SerializeField] private Transform bendTransform;
        private Vector3 bendTransformRestPos;

        private SpringDamp2 sim;

        [SerializeField] private Transform[] bendBones;
        [SerializeField] AnimationCurve bendCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 0.8f), new Keyframe(1, 1));

        private void Start()
        {
            sim = new SpringDamp2(Vector2.zero, Vector2.one * spring, Vector2.one * damp);
            bendTransformRestPos = bendTransform.localPosition;
        }

        private void Update()
        {
            sim.Bump(new Vector2(col.Forces.x, col.Forces.z) * Time.deltaTime);
            sim.Update(Vector2.zero, Time.deltaTime);
            
            Vector3 bendPos = bendTransform.parent.TransformPoint(bendTransformRestPos);
            bendPos += new Vector3(sim.Position.x, 0, sim.Position.y);
            bendTransform.position = bendPos;
        }

        private void LateUpdate()
        {
            Quaternion bendRot = Quaternion.FromToRotation(Vector3.up, new Vector3(sim.Position.x, 1, sim.Position.y));
            for (int b = bendBones.Length - 1; b >= 0; b--)
            {
                float t = b / (bendBones.Length - 1f);
                bendBones[b].rotation = Quaternion.Slerp(bendBones[b].rotation, bendRot * bendBones[b].rotation, bendCurve.Evaluate(t));
            }
        }

        public void Hit(HitInfo info)
        {
            Vector2 dir2D = new Vector2(info.direction.x, info.direction.z).normalized;
            dir2D += Random.insideUnitCircle * 0.3f;
            sim.Bump(dir2D * hitReaction);
        }
    }
}
