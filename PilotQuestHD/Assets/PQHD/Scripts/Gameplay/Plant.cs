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

        private int anim_Idle = Animator.StringToHash("Idle");

        /// <summary>
        /// Play grow animation
        /// </summary>
        public void Grow()
        {
            gameObject.SetActive(true);
            anim.Play("Grow");
            //TODO: play sound
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
            if(anim.GetCurrentAnimatorStateInfo(0).shortNameHash != anim_Idle) return;

            Inventory.I.Add(moonDrop);
            anim.SetTrigger("SpawnDrop");
            // TODO: play sound
        }
    }
}
