using UnityEngine;

namespace PQHD
{
    [CreateAssetMenu(fileName = "Loot", menuName = "PQHD/Loot")]
    public class Loot : ScriptableObject
    {
        public string id;
        public string displayName;
        public Sprite icon;
        public bool unique;
        [Tooltip("Tooltip icon for active items")]
        public Sprite useIcon;
    }
}