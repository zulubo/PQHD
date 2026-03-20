using System;
using System.Collections.Generic;
using UnityEngine;

namespace PQHD
{
    public class StoreRuntime : MonoBehaviour
    {
        public static StoreRuntime active;
        public static Action onChangeActive;

        public StoreDef def;

        public readonly Dictionary<StoreDef.Item, int> inventory = new();


        void Start()
        {
            for(int i = 0; i < def.items.Length; i++)
            {
                inventory[def.items[i]] = def.items[i].startInventory;
            }
        }

        /// <summary>
        /// Open this store's UI
        /// </summary>
        public void Open()
        {
            active = this;
            onChangeActive?.Invoke();
        }

        public void Close()
        {
            if(active == this)
            {
                active = null;
                onChangeActive?.Invoke();
            }
        }

        public void Buy(StoreDef.Item item)
        {
            if(!item.CanAfford()) return;
            if(!inventory.ContainsKey(item)) return;
            if(inventory[item] <= 0) return;

            for(int c = 0; c < item.cost.Length; c++)
            {
                item.cost[c].Spend();
            }
            Inventory.I.Add(item.loot, item.count);
            inventory[item] = inventory[item] - 1;
        }


        #region serialization

        [System.Serializable]
        public class SerializedStore
        {
            public Dictionary<string, int> inventory;
        }

        public SerializedStore Serialize()
        {
            SerializedStore serialized = new();
            serialized.inventory = new();
            for(int i = 0; i < def.items.Length; i++)
            {
                serialized.inventory[def.items[i].loot.id] = inventory[def.items[i]];
            }
            return serialized;
        }

        public void Deserialize(SerializedStore serialized)
        {
            for(int i = 0; i < def.items.Length; i++)
            {
                if(serialized != null 
                && serialized.inventory != null 
                && serialized.inventory.TryGetValue(def.items[i].loot.id, out int inv))
                {
                    inventory[def.items[i]] = inv;
                }
                else
                {
                    inventory[def.items[i]] = def.items[i].startInventory;
                }
            }
        }
        #endregion
    }
}
