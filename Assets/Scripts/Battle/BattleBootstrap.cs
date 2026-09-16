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
    /// Spawns a 3v3 test battle in Battle.unity. Wire units in the inspector, or leave empty to load defaults.
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

        void Start()
        {
            if (runFormulaSmokeTest)
            {
                string report = ElementFormulaSelfTest.RunAndFormat();
                if (report.Contains("FAIL"))
                    Debug.LogError(report);
                else
                    Debug.Log(report);
            }

            ResolveRoster();
            if (!HasParty(playerParty) || !HasParty(enemyParty))
            {
                Debug.LogError("[Battle] Missing unit definitions. Assign them on BattleBootstrap or run Tools/TS Online/Create Default Data Assets.");
                return;
            }

            ApplyCamera();
            _turns = gameObject.AddComponent<TurnManager>();
            _ui = gameObject.AddComponent<BattleUI>();

            var players = SpawnSide(playerParty, true);
            var enemies = SpawnSide(enemyParty, false);
            _turns.Setup(players, enemies);
            _turns.OnChanged += HandleBattleChanged;
            _ui.Bind(_turns);
            _turns.BeginBattle();
        }

        void HandleBattleChanged()
        {
            if (_turns == null || _turns.State != BattleState.Ended || _returning)
                return;
            _returning = true;
            if (_turns.Escaped)
                EncounterContext.LastEnd = BattleEndKind.Escape;
            else if (_turns.PlayerWon)
                EncounterContext.LastEnd = BattleEndKind.Win;
            else
                EncounterContext.LastEnd = BattleEndKind.Lose;

            if (!EncounterContext.ShouldReturnToWorld)
                return;
            StartCoroutine(ReturnToWorld());
        }

        IEnumerator ReturnToWorld()
        {
            yield return new WaitForSeconds(1.8f);
            string scene = string.IsNullOrEmpty(EncounterContext.ReturnScene) ? "World" : EncounterContext.ReturnScene;
            SceneManager.LoadScene(scene);
        }

        void ResolveRoster()
        {
            if (EncounterContext.HasPending)
            {
                if (EncounterContext.PlayerParty != null)
                    playerParty = EncounterContext.PlayerParty;
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

        List<BattleUnit> SpawnSide(UnitDefinition[] defs, bool player)
        {
            var list = new List<BattleUnit>();
            int count = 0;
            for (int i = 0; i < defs.Length; i++)
            {
                if (defs[i] != null)
                    count++;
            }

            int slot = 0;
            for (int i = 0; i < defs.Length; i++)
            {
                if (defs[i] == null)
                    continue;
                var go = new GameObject();
                var unit = go.AddComponent<BattleUnit>();
                unit.Init(defs[i], player);
                BattleWorldView.AttachPlaceholder(unit, slot, count);
                list.Add(unit);
                slot++;
            }

            return list;
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
