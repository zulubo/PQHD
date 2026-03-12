using UnityEngine;

namespace PQHD
{
    [CreateAssetMenu(fileName = "FootstepSoundProfile", menuName = "Scriptable Objects/FootstepSoundProfile")]
    public class FootstepSoundProfile : ScriptableObject
    {
        [System.Serializable]
        public class Material
        {
            public PhysicsMaterial material;
            public AudioClip[] sounds;
            [Range(0, 1)] public float volume = 1;


            private int lastPlayedSound = -1;

            public AudioClip GetRandomClip()
            {
                int i;
                do
                {
                    i = Random.Range(0, sounds.Length);
                } while (i == lastPlayedSound);

                lastPlayedSound = i;
                return sounds[i];
            }
        }

        public Material[] materials;

        public Material GetMaterial(PhysicsMaterial physicsMat)
        {
            for (int m = 0; m < materials.Length; m++)
            {
                if (materials[m].material == physicsMat) return materials[m];
            }

            return materials[0];
        }

        public void Play(PhysicsMaterial physicsMat, AudioSource source, float volumeMultiplier = 1)
        {
            Material mat = GetMaterial(physicsMat);
            source.pitch = Random.Range(0.8f, 1.2f);
            source.PlayOneShot(mat.GetRandomClip(), mat.volume * volumeMultiplier);
        }

    }
}