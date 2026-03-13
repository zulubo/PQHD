using UnityEngine;
using UnityEngine.Serialization;

namespace PQHD
{
    public class BounceOnHit : MonoBehaviour, IHittable
    {
        [SerializeField] private Transform bounceTransform;
        [Tooltip("Spring coefficients in horizontal and vertical directions.")]
        [SerializeField] private Vector2 bounceSpring = new Vector2(200, 100);
        [Tooltip("Damp coefficients in horizontal and vertical directions.")]
        [SerializeField] private Vector2 bounceDamp = new Vector2(25, 20);
        [Tooltip("Strength of bump on hit")]
        [SerializeField] private float bounceAmount = 0.5f;
        [Tooltip("Increase bounce by damage amount")]
        [SerializeField] private bool proportional = true;

        [Tooltip("Substeps for spring sim. Add more if high coefficients are unstable")]
        [SerializeField] private int simSubsteps = 2;
        
        private SpringDamp2 reactSim;
        
        private void Start()
        {
            reactSim = new SpringDamp2(Vector2.one, bounceSpring, bounceDamp);
        }
        
        public void Hit(HitInfo info)
        {
            if (info.type != HitType.Melee) return;

            float mul = 1;

            if (proportional) mul = Mathf.Sqrt(info.strength);
            
            reactSim.Bump(new Vector2(bounceAmount * mul, -bounceAmount * mul));
        }
        
        
        private void Update()
        {
            reactSim.UpdateSubstepped(Vector2.one, Time.deltaTime, simSubsteps);
            bounceTransform.transform.localScale = new Vector3(reactSim.Position.x, reactSim.Position.y, reactSim.Position.x);
        }
    }
}
