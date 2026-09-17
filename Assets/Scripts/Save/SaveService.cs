using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

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
            return File.Exists(FilePath);
        }

        public static void HydrateIfNeeded()
        {
            if (SessionHydrated)
                return;
            PartyManager.Ensure();
            if (File.Exists(FilePath))
            {
                if (TryLoad())
                    return;
                SessionHydrated = false;
                return;
            }

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
                if (string.IsNullOrWhiteSpace(json))
                {
                    DiscardCorrupt("เซฟว่าง");
                    return false;
                }

                GameSave save = JsonUtility.FromJson<GameSave>(json);
                if (!LooksValid(save))
                {
                    DiscardCorrupt("เซฟเสียหาย");
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
                DiscardCorrupt("โหลดไม่สำเร็จ");
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
                healThreshold = AutoBattleController.SavedHealThreshold,
                playerName = "",
                playerElement = 0
            };
            PartyMember lead = pm.CreatedLead;
            if (lead != null)
            {
                save.playerName = lead.ShortName;
                save.playerElement = (int)lead.Element;
            }

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
            PartyManager.Ensure().ApplySave(save.roster, save.partyOrder, save.playerName, save.playerElement);
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
            // F9 / load teleport can skip OnTriggerExit2D and leave Stay-retry flags armed.
            EncounterTrigger.ClearAllDeferredByMenu();
            return true;
        }

        public static Transform FindPlayer()
        {
            PlayerWorldController ctrl = Object.FindObjectOfType<PlayerWorldController>();
            return ctrl != null ? ctrl.transform : null;
        }

        public static bool DeleteSave()
        {
            try
            {
                if (File.Exists(FilePath))
                    File.Delete(FilePath);
                PlayerPrefs.DeleteKey(PrefsKey);
                PlayerPrefs.Save();
                PendingWorldPos = null;
                Toast("ลบเซฟแล้ว");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[Save] Delete failed: " + e.Message);
                Toast("ลบเซฟไม่สำเร็จ");
                return false;
            }
        }

        public static void ClearRuntimeStatics()
        {
            QuestTracker.Reset();
            InventoryService.Clear();
            AutoBattleController.SavedAutoAttack = false;
            AutoBattleController.SavedAutoHeal = false;
            AutoBattleController.SavedHealThreshold = AutoBattleController.DefaultHealThresholdPercent;
            EncounterContext.ResetSession();
            PendingWorldPos = null;
        }

        public static void ResetRuntimeAndCreate()
        {
            DeleteSave();
            SessionHydrated = false;
            ClearRuntimeStatics();
            PartyManager.TearDown();
            SceneManager.LoadScene("CharacterCreate");
        }

        static bool LooksValid(GameSave save)
        {
            if (save == null)
                return false;
            if (save.roster != null && save.roster.Length > 0)
                return true;
            if (save.partyOrder != null && save.partyOrder.Length > 0)
                return true;
            if (!string.IsNullOrEmpty(save.playerName))
                return true;
            return false;
        }

        static void DiscardCorrupt(string message)
        {
            Debug.LogWarning("[Save] Discarding corrupt save.");
            DeleteSave();
            SessionHydrated = false;
            Toast(string.IsNullOrEmpty(message) ? "เซฟเสียหาย" : message);
        }

        static void Toast(string msg)
        {
            LastMessage = msg;
            LastMessageAt = Time.realtimeSinceStartup;
        }
    }
}
