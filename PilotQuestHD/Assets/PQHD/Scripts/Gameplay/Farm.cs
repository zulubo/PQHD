using UnityEngine;

namespace PQHD
{
    public class Farm : MonoBehaviour
    {
        [System.Serializable]
        public class FarmPlant
        {
            public Plant plant;
            public int price;
            private bool grown;

            public bool Grown => grown;
            public void SetGrown(bool value)
            {
                 plant.gameObject.SetActive(value);
                 grown = value;
            }
            public void SetGrownWithoutActivate(bool value)
            {
                grown = value;
            }
        }

        public FarmPlant[] plants;

        public FarmPlant GetNextPlant()
        {
            for(int p = 0; p < plants.Length; p++)
            {
                if(!plants[p].Grown) return plants[p];
            }
            return null;
        }


        public bool[] Serialize()
        {
            bool[] serialized = new bool[plants.Length];
            for(int p = 0; p < plants.Length; p++)
            {
                serialized[p] = plants[p].Grown;
            }
            return serialized;
        }

        public void Deserialize(bool[] serialized)
        {
            for(int p = 0; p < plants.Length; p++)
            {
                if(serialized == null || p >= serialized.Length)
                {
                    plants[p].SetGrown(false);
                }
                else
                {
                    plants[p].SetGrown(serialized[p]);
                }
            }
        }
    }
}
