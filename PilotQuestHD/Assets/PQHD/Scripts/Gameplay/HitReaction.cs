using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;

namespace PQHD
{
    public class HitReaction : MonoBehaviour, IHittable
    {
        [SerializeField] private bool enableBounce = true;
        [SerializeField] private Transform bounceTransform;
        [Tooltip("Spring coefficient.")]
        [SerializeField] private float bounceSpring = 500;
        [Tooltip("Damp coefficient.")]
        [SerializeField] private float bounceDamp = 25;
        [Tooltip("Strength of bump on hit")]
        [SerializeField] private float bounceAmount = 0.5f;
        [FormerlySerializedAs("proportional")]
        [Tooltip("Increase bounce by damage amount")]
        [SerializeField] private bool bounceProportional = true;
        [FormerlySerializedAs("simSubsteps")]
        [Tooltip("Substeps for spring sim. Add more if high coefficients are unstable")]
        [SerializeField] private int bounceSimSubsteps = 2;
        private SpringDamp bounceSim;

        [SerializeField] private bool enableSound;
        [SerializeField] private AudioResource sound;
        [SerializeField, Range(0,1)] private float soundVolume = 1;
        [SerializeField] private float soundRadius = 30;
        
        
        private void Start()
        {
            if(enableBounce) bounceSim = new SpringDamp(1, bounceSpring, bounceDamp);
        }

        public void Hit(HitInfo info)
        {
            if (info.type != HitType.Melee) return;

            if (enableBounce)
            {
                float mul = 1;
                if (bounceProportional) mul = Mathf.Sqrt(info.strength);
                Bump(bounceAmount * mul);
            }

            if (enableSound)
            {
                Audio.I.PlaySound3D(sound, transform.position, soundRadius, soundVolume);
            }
        }

        /// <summary>
        ///  Bump the bouncy sim
        /// </summary>
        public void Bump(float velocity)
        {
            bounceSim.Bump(velocity);
        }

        private void Update()
        {
            if (enableBounce)
            {
                bounceSim.UpdateSubstepped(1, Time.deltaTime, bounceSimSubsteps);
                float invBounce = 1 - bounceSim.Position;
                bounceTransform.transform.localScale =
                    new Vector3(1 / bounceSim.Position, bounceSim.Position, 1 / bounceSim.Position);
            }
        }
    }
}
