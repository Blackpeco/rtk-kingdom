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
            sr.sprite = WorldArt.MakeShape(new Color(1f, 0.52f, 0.70f), 22, WorldArt.Shape.Blob, true,
                WorldArt.ActorMark.Patoyo);
            sr.sortingOrder = 3;
            WorldArt.AttachShadow(go.transform, 3, 1.05f);
            WorldArt.MakeLabel(go.transform, "ปาโต้เยา", new Vector3(0f, 0.62f, 0f),
                new Color(1f, 0.86f, 0.92f), 0.13f, 22);
        }
    }
}
