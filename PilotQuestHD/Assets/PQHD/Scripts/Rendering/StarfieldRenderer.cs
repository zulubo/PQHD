using UnityEngine;

namespace PQHD
{
    [ExecuteAlways]
    public class StarfieldRenderer : MonoBehaviour
    {
        ParticleSystem particleSystem;

        [SerializeField] Bounds bounds;
        [SerializeField] float starDensity = 1;
        [SerializeField] Vector2 starSizeMinMax = new Vector2(0.5f, 1.0f);
        [SerializeField] float edgeFading = 1f;

        public Vector3 scroll;

        public Gradient starColors;

        private struct Star
        {
            public Vector3 position;
            public Color color;    
            public float size;
        }
        private Star[] stars;
        private ParticleSystem.Particle[] particles;

        void Start()
        {
            particleSystem = GetComponent<ParticleSystem>();
            CreateStars();
        }

        void Update()
        {
            bool dirty = false;

            if(scroll.sqrMagnitude > 0.0001f)
            {
                Vector3 frameScroll = scroll * Time.deltaTime;
                for(int s = 0; s < stars.Length; s++)
                {
                    Vector3 pos = stars[s].position;
                    pos += frameScroll;
                    // wrapping
                    if     (pos.x > bounds.max.x) pos.x -= bounds.size.x;
                    else if(pos.x < bounds.min.x) pos.x += bounds.size.x;
                    if     (pos.y > bounds.max.y) pos.y -= bounds.size.y;
                    else if(pos.y < bounds.min.y) pos.y += bounds.size.y;
                    if     (pos.z > bounds.max.z) pos.z -= bounds.size.z;
                    else if(pos.z < bounds.min.z) pos.z += bounds.size.z;
                    stars[s].position = pos;
                }
                dirty = true;
            }

            if(dirty) UpdateParticles();
        }

        [ContextMenu("Create Stars")]
        private void CreateStars()
        {
            int starCount = Mathf.RoundToInt(bounds.size.x * bounds.size.y * bounds.size.z * starDensity);
            stars = new Star[starCount];
            for(int s = 0; s < starCount; s++)
            {
                stars[s] = new Star()
                {
                    position = new Vector3(Random.Range(bounds.min.x, bounds.max.x), Random.Range(bounds.min.y, bounds.max.y), Random.Range(bounds.min.z, bounds.max.z)),
                    color = starColors.Evaluate(Random.value),
                    size = Random.Range(starSizeMinMax.x, starSizeMinMax.y)
                };
            }
            particleSystem.Play();
            particleSystem.Pause();
            particles = new ParticleSystem.Particle[starCount];
            UpdateParticles();
            
        }

        private void UpdateParticles()
        {
            for(int s = 0; s < stars.Length; s++)
            {
                particles[s].position = stars[s].position;
                particles[s].startSize = stars[s].size;

                // edge fading
                Color color = stars[s].color;
                float edgeDist = Mathf.Min(bounds.max.x - particles[s].position.x, particles[s].position.x - bounds.min.x, 
                                            bounds.max.y - particles[s].position.y, particles[s].position.y - bounds.min.y, 
                                            bounds.max.z - particles[s].position.z, particles[s].position.z - bounds.min.z);
                float edgeFade = Mathf.Clamp01(edgeDist / edgeFading);
                color *= edgeFade;

                particles[s].startColor = color;
            }

            particleSystem.SetParticles(particles);
        }
    }
}
