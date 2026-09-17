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
            BuildLabels();
            GameObject player = BuildPlayer();
            PatoyoFollower.Spawn(player.transform);
            CityQuestNpc.Spawn();
            LoadEnemyRefs();
            BuildForestEncounters();
            gameObject.AddComponent<WorldHUD>();
            gameObject.AddComponent<PartyWorldUI>();

            if (EncounterContext.ShouldReturnToWorld || EncounterContext.LastEnd != BattleEndKind.None)
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
            sr.sprite = MakeQuad(body, 16);
            sr.sortingOrder = 5;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.38f;
            go.AddComponent<PlayerWorldController>();
            var label = new GameObject("Name");
            label.transform.SetParent(go.transform, false);
            label.transform.localPosition = new Vector3(0f, 0.7f, 0f);
            var tm = label.AddComponent<TextMesh>();
            tm.text = lead != null && !string.IsNullOrEmpty(lead.ShortName) ? lead.ShortName : "จูล่ง";
            tm.characterSize = 0.16f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.fontSize = 24;
            tm.color = Color.white;
            return go;
        }

        void BuildForestEncounters()
        {
            SpawnWanderer("Wolf", new Vector3(4.4f, 1.6f, 0f), new Color(0.40f, 0.82f, 0.55f),
                new[] { forestWolf, forestWolf });
            SpawnWanderer("Bandit", new Vector3(6.2f, -0.4f, 0f), new Color(0.86f, 0.24f, 0.18f),
                new[] { mountainBandit, swampFrog });
            SpawnWanderer("Frog", new Vector3(5.0f, -2.0f, 0f), new Color(0.22f, 0.52f, 0.82f),
                new[] { swampFrog, forestWolf, mountainBandit });
        }

        void SpawnWanderer(string id, Vector3 pos, Color color, UnitDefinition[] pack)
        {
            var go = new GameObject("Encounter_" + id);
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = MakeQuad(color, 14);
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
        }

        void BuildGround()
        {
            CreateQuad("CityGround", new Vector3(-5.5f, 0f, 1f), new Vector3(11f, 9f, 1f), new Color(0.22f, 0.24f, 0.28f), 0);
            CreateQuad("ForestGround", new Vector3(5.2f, 0f, 1f), new Vector3(11f, 9f, 1f), new Color(0.14f, 0.28f, 0.16f), 0);
            CreateQuad("Road", new Vector3(-0.2f, 0f, 1f), new Vector3(2.2f, 2.4f, 1f), new Color(0.32f, 0.30f, 0.22f), 1);
        }

        void BuildLabels()
        {
            WorldLabel("เมือง (ปลอดภัย)", new Vector3(-5.6f, 3.6f, 0f));
            WorldLabel("ป่า (สุ่มสู้)", new Vector3(5.0f, 3.6f, 0f));
        }

        static void WorldLabel(string text, Vector3 pos)
        {
            var go = new GameObject(text);
            go.transform.position = pos;
            var tm = go.AddComponent<TextMesh>();
            tm.text = text;
            tm.characterSize = 0.22f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.fontSize = 28;
            tm.color = Color.white;
        }

        static void CreateQuad(string name, Vector3 pos, Vector3 scale, Color color, int sort)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = scale;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = MakeQuad(color, 8);
            sr.sortingOrder = sort;
        }

        static Sprite MakeQuad(Color color, int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = color;
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        static void ApplyCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
                return;
            cam.orthographic = true;
            cam.orthographicSize = 5.4f;
            cam.backgroundColor = new Color(0.08f, 0.10f, 0.11f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.transform.position = new Vector3(-4f, 0f, -10f);
        }
    }
}
