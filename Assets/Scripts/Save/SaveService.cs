using System.IO;
using UnityEngine;

namespace TsOnline
{
    /// <summary>JSON save under persistentDataPath. Hydrate once per play session.</summary>
    public static class SaveService
    {
        public const string FileName = "ts_online_save.json";
        public const string PrefsKey = "tso_has_save";

        public static bool SessionHydrated;
        public static string LastMessage = "";
        public static float LastMessageAt;

        public static string FilePath
        {
            get { return Path.Combine(Application.persistentDataPath, FileName); }
        }

        public static bool Exists()
        {
            return File.Exists(FilePath) || PlayerPrefs.GetInt(PrefsKey, 0) == 1 && File.Exists(FilePath);
        }

        public static void HydrateIfNeeded()
        {
            if (SessionHydrated)
                return;
            PartyManager.Ensure();
            if (File.Exists(FilePath))
                TryLoad();
            SessionHydrated = true;
        }

        public static bool TryLoad()
        {
            if (!File.Exists(FilePath))
            {
                Toast("ยังไม่มีไฟล์เซฟ");
                return false;
            }

            try
            {
                string json = File.ReadAllText(FilePath);
                GameSave save = JsonUtility.FromJson<GameSave>(json);
                if (save == null)
                {
                    Toast("เซฟเสียหาย");
                    return false;
                }

                Apply(save);
                SessionHydrated = true;
                Toast("โหลดแล้ว");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[Save] Load failed: " + e.Message);
                Toast("โหลดไม่สำเร็จ");
                return false;
            }
        }

        public static bool Save(Vector3? worldPos)
        {
            try
            {
                GameSave save = Capture(worldPos);
                string json = JsonUtility.ToJson(save, true);
                File.WriteAllText(FilePath, json);
                PlayerPrefs.SetInt(PrefsKey, 1);
                PlayerPrefs.Save();
                Toast("บันทึกแล้ว");
                Debug.Log("[Save] Wrote " + FilePath);
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[Save] Save failed: " + e.Message);
                Toast("บันทึกไม่สำเร็จ");
                return false;
            }
        }

        public static GameSave Capture(Vector3? worldPos)
        {
            PartyManager pm = PartyManager.Ensure();
            var save = new GameSave
            {
                version = 1,
                questPhase = (int)QuestTracker.Phase,
                questWins = QuestTracker.ForestWins,
                roster = pm.ExportRoster(),
                partyOrder = pm.ExportPartyOrder(),
                inventory = InventoryService.Export(),
                autoAttack = AutoBattleController.SavedAutoAttack,
                autoHeal = AutoBattleController.SavedAutoHeal,
                healThreshold = AutoBattleController.SavedHealThreshold
            };

            if (worldPos.HasValue)
            {
                save.hasWorldPos = true;
                save.worldX = worldPos.Value.x;
                save.worldY = worldPos.Value.y;
            }
            else
            {
                Transform player = FindPlayer();
                if (player != null)
                {
                    save.hasWorldPos = true;
                    save.worldX = player.position.x;
                    save.worldY = player.position.y;
                }
                else
                    CopyPreviousWorldPos(save);
            }

            return save;
        }

        public static void Apply(GameSave save)
        {
            PartyManager.Ensure().ApplySave(save.roster, save.partyOrder);
            InventoryService.Apply(save.inventory);
            QuestTracker.Apply(save.questPhase, save.questWins);
            AutoBattleController.SavedAutoAttack = save.autoAttack;
            AutoBattleController.SavedAutoHeal = save.autoHeal;
            AutoBattleController.SavedHealThreshold = save.healThreshold > 0f
                ? save.healThreshold
                : AutoBattleController.DefaultHealThresholdPercent;
            PendingWorldPos = save.hasWorldPos
                ? new Vector3(save.worldX, save.worldY, 0f)
                : (Vector3?)null;
        }

        static void CopyPreviousWorldPos(GameSave save)
        {
            if (!File.Exists(FilePath))
                return;
            try
            {
                GameSave prev = JsonUtility.FromJson<GameSave>(File.ReadAllText(FilePath));
                if (prev == null || !prev.hasWorldPos)
                    return;
                save.hasWorldPos = true;
                save.worldX = prev.worldX;
                save.worldY = prev.worldY;
            }
            catch (System.Exception)
            {
            }
        }

        public static Vector3? PendingWorldPos;

        public static bool TryApplyWorldPosition(Transform player)
        {
            if (player == null || !PendingWorldPos.HasValue)
                return false;
            player.position = PendingWorldPos.Value;
            return true;
        }

        public static Transform FindPlayer()
        {
            PlayerWorldController ctrl = Object.FindObjectOfType<PlayerWorldController>();
            return ctrl != null ? ctrl.transform : null;
        }

        static void Toast(string msg)
        {
            LastMessage = msg;
            LastMessageAt = Time.realtimeSinceStartup;
        }
    }
}
