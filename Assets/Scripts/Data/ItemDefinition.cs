using UnityEngine;

namespace TsOnline
{
    /// <summary>Stub fields for Step 1. Items are not used in combat yet.</summary>
    [CreateAssetMenu(fileName = "Item", menuName = "TS Online/Data/Item Definition", order = 3)]
    public class ItemDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        public string displayNameThai;
        [TextArea(2, 4)]
        public string description;
        public int stackLimit = 99;
        public Sprite icon;
    }
}
