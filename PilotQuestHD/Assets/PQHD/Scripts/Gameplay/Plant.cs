using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace PQHD
{
    public class Plant : MonoBehaviour
    {
        [SerializeField] float dropTime = 2f;
        [SerializeField] Loot moonDrop;
        [SerializeField] Animator anim;
        [SerializeField] AudioResource spawnDropSound;
        [SerializeField] AudioResource growSound;
        [SerializeField] private ParticleSystem growParticles;
        [SerializeField] private HitReaction reaction;

        [SerializeField] private float soundRadius = 15;

        private BoolTimer growing;

        /// <summary>
        /// Play grow animation
        /// </summary>
        public void Grow()
        {
            gameObject.SetActive(true);
            anim.Play("Grow", 0);
            growing.Set(1f);
            //TODO: play sound
            growParticles?.Play();
        }

        void OnEnable()
        {
            StartCoroutine(SpawnCoroutine());
        }

        IEnumerator SpawnCoroutine()
        {
            yield return new WaitForSeconds(dropTime * Random.value);
            
            while(true)
            {
                SpawnDrop();
                yield return new WaitForSeconds(dropTime);
            }
        }

        void SpawnDrop()
        {
            if(growing) return;

            Inventory.I.Add(moonDrop);
            anim.SetTrigger("SpawnDrop");
            reaction.Bump(4);
            Audio.I.PlaySound3D(moonDrop.pickupSound, transform.position, soundRadius, 0.7f);
        }
    }
}
