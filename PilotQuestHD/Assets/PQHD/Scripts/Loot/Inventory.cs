using System;
using System.Collections.Generic;
using UnityEngine;

namespace PQHD
{
    public class Inventory : MonoBehaviour
    {
        public static Inventory I;
        
        public LootDatabase database;

        public Dictionary<Loot, int> contents = new();

        public static Action onChanged;

        /// <summary>
        /// Get the number of a loot item in inventory
        /// </summary>
        public int GetCount(Loot loot)
        {
            if (contents.TryGetValue(loot, out int count))
            {
                return count;
            }
            return 0;
        }

        /// <summary>
        /// Add loot to inventory
        /// </summary>
        public void Add(Loot loot, int count = 1)
        {
            contents.TryGetValue(loot, out int haveCount);
            haveCount += count;
            int capacity = GetCapacity(loot);
            if(capacity > 0)
            {
                if(haveCount > capacity) haveCount = capacity;
            }
            if (loot.unique && haveCount > 1) haveCount = 1;
            contents[loot] = haveCount;
            onChanged?.Invoke();
        }

        /// <summary>
        /// Remove loot from inventory
        /// </summary>
        public void Remove(Loot loot, int count = 1)
        {
            if (contents.TryGetValue(loot, out int haveCount))
            {
                haveCount -= count;
                if (haveCount < 0) haveCount = 0;
                contents[loot] = haveCount;
                onChanged?.Invoke();
            }
        }

        /// <summary>
        /// Remove all of one loot item
        /// </summary>
        public void RemoveAll(Loot loot)
        {
            if (contents.ContainsKey(loot))
            {
                contents[loot] = 0;
                onChanged?.Invoke();
            }
        }

        /// <summary>
        /// Remove all posessions
        /// </summary>
        public void RemoveAll()
        {
            foreach (Loot loot in database.loot)
            {
                contents[loot] = 0;
            }
            onChanged?.Invoke();
        }

        private void Awake()
        {
            I = this;
            contents = new();
            for (int l = 0; l < database.loot.Length; l++)
            {
                contents.Add(database.loot[l], 0);
            }
        }

        /// <summary>
        /// Get the capacity of the inventory for a loot type
        /// </summary>
        public int GetCapacity(Loot loot)
        {
            return loot.inventoryCapacity;
        }

        [System.Serializable]
        public struct SerializedInventory
        {
            public Dictionary<string, int> contents;
        }

        public SerializedInventory Serialize()
        {
            SerializedInventory saveData = new SerializedInventory();
            saveData.contents = new();

            foreach (KeyValuePair<Loot, int> content in contents)
            {
                saveData.contents.Add(content.Key.id, content.Value);
            }
            
            return saveData;
        }

        public void Deserialize(SerializedInventory serialized)
        {
            // clear 
            RemoveAll();
            
            if(serialized.contents == null) return;
            foreach (KeyValuePair<string, int> loadedContent in serialized.contents)
            {
                Loot loot = database.FindByID(loadedContent.Key);
                if (loot != null && contents.ContainsKey(loot)) contents[loot] = loadedContent.Value;
            }
            onChanged?.Invoke();
        }
    }
}