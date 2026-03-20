using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PQHD
{
    [CreateAssetMenu(fileName = "StoreDef", menuName = "PQHD/StoreDef")]
    public class StoreDef : ScriptableObject
    {
        public string header;

        [Tooltip("Currencies used in the store. Should include all the ones in the prices for items")]
        public Loot[] currencies;
        
        [System.Serializable]
        public class Cost
        {
            public Loot currency;
            public int amount;

            public bool CanAfford()
            {
                return Inventory.I.GetCount(currency) > amount;
            }

            public void Spend()
            {
                Inventory.I.Remove(currency, amount);
            }
        }

        [System.Serializable]
        public class Item
        {
            public Loot loot;
            [Tooltip("How many to buy")]
            public int count = 1;
            public Cost[] cost;

            [Tooltip("How many in store")]
            public int startInventory = 1;

            public int? GetCost(Loot currency)
            {
                return System.Array.Find(cost, c => c.currency == currency)?.amount;
            }

            public bool CanAfford()
            {
                for(int c = 0; c < cost.Length; c++)
                {
                    if(!cost[c].CanAfford()) return false;
                }
                return true;
            }
        }

        public Item[] items;


    }
}
