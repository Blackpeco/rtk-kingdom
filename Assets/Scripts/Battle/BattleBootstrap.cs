using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TsOnline
{
    /// <summary>
    /// World encounters spawn the PartyManager roster and persist the save.
    /// Direct Play on Battle.unity uses the inspector / DefaultEncounter 3v3 — it does not hydrate a save,
    /// does not write ts_online_save.json, and does not set EncounterContext.LastEnd.
    /// </summary>
    public class BattleBootstrap : MonoBehaviour
    {
        [Header("Party (left) — defaults: Zhao Yun, Guan Yu, Zhuge Liang")]
        public UnitDefinition[] playerParty;

        [Header("Enemies (right) — defaults: Forest Wolf, Mountain Bandit, Swamp Frog")]
        public UnitDefinition[] enemyParty;

        [Header("Optional Resources fallback: Battle/DefaultEncounter")]
        public BattleTestConfig encounterConfig;

        [Tooltip("Keep Step 1 formula smoke-test log on Play.")]
        public bool runFormulaSmokeTest = true;

        TurnManager _turns;
        BattleUI _ui;
        bool _returning;
        /// <summary>Captured at Start from <see cref="EncounterContext.HasPending"/>. Leftover ShouldReturnToWorld must not count.</summary>
        bool _cameFromWorld;

        void Start()
        {
            _cameFromWorld = EncounterContext.HasPending;
            // Sandbox Battle.unity must not leave a leftover LastEnd for the next World play.
            if (!_cameFromWorld)
                EncounterContext.LastEnd = BattleEndKind.None;

            if (runFormulaSmokeTest)
            {
                string report = ElementFormulaSelfTest.RunAndFormat();
                if (report.Contains("FAIL"))
                    Debug.LogError(report);
                else
                    Debug.Log(report);
            }

            PatoyoHelper.ResetForBattle();

            ResolveRoster();
            if (!HasParty(playerParty) || !HasParty(enemyParty))
            {
                Debug.LogError("[Battle] Missing unit definitions. Assign them on BattleBootstrap or run Tools/TS Online/Create Default Data Assets.");
                return;
            }

            ApplyCamera();
            BattleWorldView.BuildArena();
            PartyManager.Ensure();
            _turns = gameObject.AddComponent<TurnManager>();
            _ui = gameObject.AddComponent<BattleUI>();
            var auto = gameObject.AddComponent<AutoBattleController>();

            var players = _cameFromWorld ? SpawnPlayersFromParty() : SpawnSide(playerParty, true, null);
            var enemies = SpawnSide(enemyParty, false, null);
            _turns.Setup(players, enemies);
            _turns.OnChanged += HandleBattleChanged;
            _ui.Bind(_turns, auto);
            auto.Bind(_turns);
            PatoyoHelper.SpawnBattleView();
            _turns.BeginBattle();
        }

        void HandleBattleChanged()
        {
            if (_turns == null || _turns.State != BattleState.Ended || _returning)
                return;
            _returning = true;
            if (_cameFromWorld)
            {
                if (_turns.Escaped)
                    EncounterContext.LastEnd = BattleEndKind.Escape;
                else if (_turns.PlayerWon)
                    EncounterContext.LastEnd = BattleEndKind.Win;
                else
                    EncounterContext.LastEnd = BattleEndKind.Lose;
            }

            PartyManager pm = PartyManager.Ensure();
            pm.WriteBackFromBattle(_turns.PlayerUnits);
            if (_turns.PlayerWon)
            {
                pm.AwardWinExp(enemyParty);
                if (_cameFromWorld)
                    QuestTracker.NotifyForestWin();
            }
            if (_ui != null)
                _ui.NoteRewards(pm.LastRewardSummary);
            PersistPartyIfWorldEncounter();

            System.Action goHome = _cameFromWorld
                ? () => StartCoroutine(ReturnToWorld())
                : (System.Action)null;

            if (pm.PendingPoints() > 0 && _ui != null)
                _ui.OpenLevelUp(goHome, _cameFromWorld);
            else if (goHome != null)
                StartCoroutine(ReturnToWorld());
        }

        void PersistPartyIfWorldEncounter()
        {
            if (!_cameFromWorld)
                return;
            SaveService.Save(EncounterContext.ReturnPosition);
        }

        IEnumerator ReturnToWorld()
        {
            yield return new WaitForSeconds(1.8f);
            string scene = string.IsNullOrEmpty(EncounterContext.ReturnScene) ? "World" : EncounterContext.ReturnScene;
            SceneManager.LoadScene(scene);
        }

        static bool FromWorldEncounter()
        {
            return EncounterContext.HasPending;
        }

        void ResolveRoster()
        {
            if (FromWorldEncounter())
            {
                PartyManager pm = PartyManager.Ensure();
                if (pm.Party.Count > 0)
                    playerParty = pm.ActiveDefinitions();
            }

            if (EncounterContext.HasPending)
            {
                if (EncounterContext.Enemies != null)
                    enemyParty = EncounterContext.Enemies;
            }

            if (encounterConfig == null)
                encounterConfig = Resources.Load<BattleTestConfig>("Battle/DefaultEncounter");

            if (!HasParty(playerParty) && encounterConfig != null)
                playerParty = encounterConfig.playerParty;
            if (!HasParty(enemyParty) && encounterConfig != null)
                enemyParty = encounterConfig.enemyParty;

            if (!HasParty(playerParty))
                playerParty = LoadDefaults(true);
            if (!HasParty(enemyParty))
                enemyParty = LoadDefaults(false);
        }

        static bool HasParty(UnitDefinition[] list)
        {
            if (list == null || list.Length == 0)
                return false;
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i] != null)
                    return true;
            }

            return false;
        }

        static UnitDefinition[] LoadDefaults(bool players)
        {
            if (players)
            {
                return new[]
                {
                    LoadUnit("Assets/Data/Generals/ZhaoYun.asset"),
                    LoadUnit("Assets/Data/Generals/GuanYu.asset"),
                    LoadUnit("Assets/Data/Generals/ZhugeLiang.asset")
                };
            }

            return new[]
            {
                LoadUnit("Assets/Data/Monsters/ForestWolf.asset"),
                LoadUnit("Assets/Data/Monsters/MountainBandit.asset"),
                LoadUnit("Assets/Data/Monsters/SwampFrog.asset")
            };
        }

        static UnitDefinition LoadUnit(string assetPath)
        {
            string key = ResourceKey(assetPath);
            if (!string.IsNullOrEmpty(key))
            {
                UnitDefinition fromResources = Resources.Load<UnitDefinition>(key);
                if (fromResources != null)
                    return fromResources;
            }

#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<UnitDefinition>(assetPath);
#else
            return null;
#endif
        }

        static string ResourceKey(string assetPath)
        {
            const string prefix = "Assets/Resources/";
            if (string.IsNullOrEmpty(assetPath) || !assetPath.StartsWith(prefix))
                return null;
            string trimmed = assetPath.Substring(prefix.Length);
            if (trimmed.EndsWith(".asset"))
                trimmed = trimmed.Substring(0, trimmed.Length - 6);
            return trimmed;
        }

        List<BattleUnit> SpawnPlayersFromParty()
        {
            PartyManager pm = PartyManager.Ensure();
            if (pm.Party.Count > 0)
                return SpawnSide(null, true, pm.Party);
            return SpawnSide(playerParty, true, null);
        }

        List<BattleUnit> SpawnSide(UnitDefinition[] defs, bool player, IList<PartyMember> members)
        {
            var list = new List<BattleUnit>();
            int count = 0;
            if (members != null)
            {
                for (int i = 0; i < members.Count; i++)
                {
                    if (members[i] != null && members[i].definition != null)
                        count++;
                }
            }
            else if (defs != null)
            {
                for (int i = 0; i < defs.Length; i++)
                {
                    if (defs[i] != null)
                        count++;
                }
            }

            int slot = 0;
            if (members != null)
            {
                for (int i = 0; i < members.Count; i++)
                {
                    PartyMember member = members[i];
                    if (member == null || member.definition == null)
                        continue;
                    list.Add(MakeUnit(member.definition, player, member, slot, count));
                    slot++;
                }

                return list;
            }

            if (defs == null)
                return list;
            for (int i = 0; i < defs.Length; i++)
            {
                if (defs[i] == null)
                    continue;
                list.Add(MakeUnit(defs[i], player, null, slot, count));
                slot++;
            }

            return list;
        }

        static BattleUnit MakeUnit(UnitDefinition def, bool player, PartyMember member, int slot, int count)
        {
            var go = new GameObject();
            var unit = go.AddComponent<BattleUnit>();
            unit.Init(def, player, member);
            BattleWorldView.AttachPlaceholder(unit, slot, count);
            return unit;
        }

        static void ApplyCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
                return;
            cam.orthographic = true;
            cam.orthographicSize = 5.2f;
            cam.backgroundColor = new Color(0.07f, 0.08f, 0.10f, 1f);
            cam.clearFlags = CameraClearFlags.SolidColor;
        }
    }
}
