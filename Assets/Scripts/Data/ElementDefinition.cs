using UnityEngine;

namespace TsOnline
{
    [CreateAssetMenu(fileName = "Element", menuName = "TS Online/Data/Element Definition", order = 0)]
    public class ElementDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        public string displayNameThai;
        public ElementType element;
        public Sprite icon;
        public Color color = Color.white;
        [TextArea(2, 6)]
        public string roleBlurb;
    }
}
