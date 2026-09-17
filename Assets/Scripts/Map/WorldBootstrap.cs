using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TsOnline
{
    /// <summary>
    /// Builds a small city (safe, left) + forest (encounters, right) and the player at runtime.
    /// </summary>
    public class WorldBootstrap : MonoBehaviour
    {
        public static bool EncountersLocked { get; private set; }

        [Header("Fallback enemy refs if Resources fail")]
        public UnitDefinition forestWolf;
        public UnitDefinition mountainBandit;
        public UnitDefinition swampFrog;

        const float ReturnGraceSeconds = 1.6f;

        void Start()
        {
            PartyManager.Ensure();
            SaveService.HydrateIfNeeded();
            ApplyCamera();
            BuildGround();
            BuildDecor();
            BuildLabels();
            GameObject player = BuildPlayer();
            PatoyoFollower.Spawn(player.transform);
            CityQuestNpc.Spawn();
            LoadEnemyRefs();
            BuildForestEncounters();
            gameObject.AddComponent<WorldHUD>();
            gameObject.AddComponent<PartyWorldUI>();

            // Real World→Battle handoff only. A leftover LastEnd (or sandbox Battle win) must not fake a return+save.
            if (EncounterContext.ShouldReturnToWorld)
            {
                Vector3 back = EncounterContext.ReturnPosition + Vector3.left * 1.25f;
                player.transform.position = back;
                EncounterContext.MarkReturned();
                SaveService.PendingWorldPos = null;
                SaveService.Save(player.transform.position);
                EncountersLocked = true;
                Invoke(nameof(UnlockEncounters), ReturnGraceSeconds);
            }
            else
            {
                EncountersLocked = false;
                SaveService.TryApplyWorldPosition(player.transform);
            }

            var camFollow = Camera.main != null ? Camera.main.gameObject.AddComponent<WorldCameraFollow>() : null;
            if (camFollow != null)
                camFollow.target = player.transform;
        }

        void UnlockEncounters()
        {
            EncountersLocked = false;
        }

        void LoadEnemyRefs()
        {
            if (forestWolf == null)
                forestWolf = LoadMonster("Assets/Data/Monsters/ForestWolf.asset", 0);
            if (mountainBandit == null)
                mountainBandit = LoadMonster("Assets/Data/Monsters/MountainBandit.asset", 1);
            if (swampFrog == null)
                swampFrog = LoadMonster("Assets/Data/Monsters/SwampFrog.asset", 2);
        }

        static UnitDefinition LoadMonster(string path, int encounterIndex)
        {
#if UNITY_EDITOR
            var fromEditor = AssetDatabase.LoadAssetAtPath<UnitDefinition>(path);
            if (fromEditor != null)
                return fromEditor;
#endif
            BattleTestConfig cfg = Resources.Load<BattleTestConfig>("Battle/DefaultEncounter");
            if (cfg != null && cfg.enemyParty != null && encounterIndex < cfg.enemyParty.Length)
                return cfg.enemyParty[encounterIndex];
            return null;
        }

        GameObject BuildPlayer()
        {
            var go = new GameObject("Player");
            go.transform.position = new Vector3(-6.2f, 0f, 0f);
            PartyMember lead = PartyManager.Ensure().Party.Count > 0 ? PartyManager.Ensure().Party[0] : null;
            Color body = lead != null ? CreatedHero.ColorOf(lead.Element) : new Color(0.95f, 0.82f, 0.28f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = WorldArt.MakeShape(body, 24, WorldArt.Shape.Diamond);
            sr.sortingOrder = 5;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.38f;
            go.AddComponent<PlayerWorldController>();
            string name = lead != null && !string.IsNullOrEmpty(lead.ShortName) ? lead.ShortName : "จูล่ง";
            string el = lead != null ? CreatedHero.Thai(lead.Element) : "";
            WorldArt.MakeLabel(go.transform, name, new Vector3(0f, 0.78f, 0f), Color.white, 0.15f, 26);
            if (!string.IsNullOrEmpty(el))
                WorldArt.MakeLabel(go.transform, "หัวหน้า · " + el, new Vector3(0f, 0.54f, 0f),
                    Color.Lerp(body, Color.white, 0.4f), 0.11f, 22);
            return go;
        }

        void BuildForestEncounters()
        {
            SpawnWanderer("Wolf", "หมาป่า", new Vector3(4.4f, 1.6f, 0f), new Color(0.34f, 0.86f, 0.48f),
                WorldArt.Shape.Diamond, new[] { forestWolf, forestWolf });
            SpawnWanderer("Bandit", "โจร", new Vector3(6.2f, -0.4f, 0f), new Color(0.92f, 0.28f, 0.18f),
                WorldArt.Shape.Square, new[] { mountainBandit, swampFrog });
            SpawnWanderer("Frog", "กบ", new Vector3(5.0f, -2.0f, 0f), new Color(0.18f, 0.58f, 0.92f),
                WorldArt.Shape.Circle, new[] { swampFrog, forestWolf, mountainBandit });
        }

        void SpawnWanderer(string id, string thai, Vector3 pos, Color color, WorldArt.Shape shape, UnitDefinition[] pack)
        {
            var go = new GameObject("Encounter_" + id);
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = WorldArt.MakeShape(color, 20, shape);
            sr.sortingOrder = 4;
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.45f;
            var trigger = go.AddComponent<EncounterTrigger>();
            trigger.enemies = pack;
            trigger.encounterName = "ป่า — " + id;
            var wander = go.AddComponent<WanderingMonster>();
            wander.range = 0.85f;
            wander.speed = 0.9f;
            WorldArt.MakeLabel(go.transform, thai, new Vector3(0f, 0.62f, 0f), Color.Lerp(color, Color.white, 0.45f), 0.12f, 22);
        }

        void BuildGround()
        {
            CreateGround("CityGround", new Vector3(-5.5f, 0f, 1f), new Vector3(11f, 9f, 1f), true, 0);
            CreateGround("ForestGround", new Vector3(5.2f, 0f, 1f), new Vector3(11f, 9f, 1f), false, 0);
            var road = new GameObject("Road");
            road.transform.position = new Vector3(-0.2f, 0f, 1f);
            road.transform.localScale = new Vector3(2.4f, 2.6f, 1f);
            var roadSr = road.AddComponent<SpriteRenderer>();
            roadSr.sprite = WorldArt.MakeQuad(new Color(0.58f, 0.44f, 0.28f), 16);
            roadSr.sortingOrder = 1;
        }

        void BuildDecor()
        {
            PlaceDecor("HouseA", new Vector3(-8.4f, 2.8f, 0f), new Vector3(1.5f, 1.7f, 1f),
                new Color(0.52f, 0.36f, 0.30f), WorldArt.Shape.Square, 2);
            PlaceDecor("HouseB", new Vector3(-8.6f, -2.2f, 0f), new Vector3(1.3f, 1.5f, 1f),
                new Color(0.40f, 0.38f, 0.44f), WorldArt.Shape.Square, 2);
            PlaceDecor("Well", new Vector3(-4.0f, 2.2f, 0f), new Vector3(0.7f, 0.7f, 1f),
                new Color(0.42f, 0.46f, 0.52f), WorldArt.Shape.Circle, 2);
            PlaceDecor("TreeA", new Vector3(8.6f, 2.9f, 0f), new Vector3(1.3f, 1.6f, 1f),
                new Color(0.08f, 0.28f, 0.10f), WorldArt.Shape.Triangle, 2);
            PlaceDecor("TreeB", new Vector3(8.8f, -2.6f, 0f), new Vector3(1.2f, 1.5f, 1f),
                new Color(0.10f, 0.32f, 0.12f), WorldArt.Shape.Triangle, 2);
            PlaceDecor("TreeC", new Vector3(3.4f, 3.1f, 0f), new Vector3(1.0f, 1.3f, 1f),
                new Color(0.07f, 0.24f, 0.10f), WorldArt.Shape.Triangle, 2);
        }

        void BuildLabels()
        {
            PlaceZoneChip("เมือง (ปลอดภัย)", new Vector3(-5.6f, 3.7f, 0f), new Color(0.55f, 0.50f, 0.40f));
            PlaceZoneChip("ป่า (สุ่มสู้)", new Vector3(5.0f, 3.7f, 0f), new Color(0.18f, 0.40f, 0.20f));
        }

        static void PlaceZoneChip(string text, Vector3 pos, Color chip)
        {
            var go = new GameObject(text);
            go.transform.position = pos;
            var chipGo = new GameObject("Chip");
            chipGo.transform.SetParent(go.transform, false);
            chipGo.transform.localScale = new Vector3(3.6f, 0.58f, 1f);
            var sr = chipGo.AddComponent<SpriteRenderer>();
            sr.sprite = WorldArt.MakeQuad(new Color(chip.r, chip.g, chip.b, 0.92f), 12);
            sr.sortingOrder = 3;
            WorldArt.MakeLabel(go.transform, text, Vector3.zero, Color.white, 0.16f, 28);
        }

        static void PlaceDecor(string name, Vector3 pos, Vector3 scale, Color color, WorldArt.Shape shape, int sort)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = scale;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = WorldArt.MakeShape(color, 16, shape, false);
            sr.sortingOrder = sort;
        }

        static void CreateGround(string name, Vector3 pos, Vector3 scale, bool city, int sort)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = scale;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = WorldArt.MakeGround(city);
            sr.sortingOrder = sort;
        }

        static void ApplyCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
                return;
            cam.orthographic = true;
            cam.orthographicSize = 5.4f;
            cam.backgroundColor = new Color(0.07f, 0.10f, 0.08f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.transform.position = new Vector3(-4f, 0f, -10f);
        }
    }
}
