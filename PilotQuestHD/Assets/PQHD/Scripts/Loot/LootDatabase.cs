using UnityEngine;

namespace PQHD
{
    [CreateAssetMenu(fileName = "LootDatabase", menuName = "Scriptable Objects/LootDatabase")]
    public class LootDatabase : ScriptableObject
    {
        public Loot[] loot;

        public Loot FindByID(string id)
        {
            return System.Array.Find(loot, l => l.id == id);
        }
        
        #if UNITY_EDITOR
        [ContextMenu("Find Loot Assets")]
        void FindLootAssets()
        {
            string[] lootAssetPaths = UnityEditor.AssetDatabase.FindAssets("t:Loot");
            loot = new Loot[lootAssetPaths.Length];
            for (int l = 0; l < lootAssetPaths.Length; l++)
            {
                loot[l] = UnityEditor.AssetDatabase.LoadAssetAtPath(lootAssetPaths[l], typeof(Loot)) as Loot;
            }
        }
        #endif
    }
}
