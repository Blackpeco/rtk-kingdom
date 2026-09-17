using UnityEngine;

namespace TsOnline
{
    /// <summary>
    /// Battle helper for ปาโต้เยา. Not a BattleUnit / party slot. Two small heals per fight.
    /// </summary>
    public static class PatoyoHelper
    {
        public const int MaxCharges = 2;
        public const int HealAmount = 22;

        public static int ChargesLeft = MaxCharges;

        public static void ResetForBattle()
        {
            ChargesLeft = MaxCharges;
        }

        public static bool TryHeal(BattleUnit target)
        {
            if (ChargesLeft <= 0 || target == null || !target.IsAlive)
                return false;
            target.HealHp(HealAmount);
            ChargesLeft--;
            return true;
        }

        public static void SpawnBattleView()
        {
            var go = new GameObject("PatoyoHelper");
            go.transform.position = new Vector3(-6.6f, -3.35f, 0f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = WorldArt.MakeQuad(new Color(1f, 0.58f, 0.72f), 10);
            sr.sortingOrder = 3;
            var label = new GameObject("Name");
            label.transform.SetParent(go.transform, false);
            label.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            var tm = label.AddComponent<TextMesh>();
            tm.text = "ปาโต้เยา";
            tm.characterSize = 0.14f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.fontSize = 22;
            tm.color = new Color(1f, 0.85f, 0.9f);
        }
    }
}
