using UnityEngine;
using UnityEngine.SceneManagement;

namespace TsOnline
{
    /// <summary>
    /// Overlap with the player starts a Battle using EncounterContext.
    /// Skips while <see cref="PlayerWorldController.MenuOpen"/> (party panel or ยายเมือง dialog)
    /// without consuming the trigger. Stay retries only after that menu defer — not when
    /// the post-fight grace lock expires while the player is standing still.
    /// Wanderers may still roam.
    /// F9 / any teleport can skip Exit2D; deferred flags clear when the player is no longer
    /// geometrically overlapping, and when <see cref="ClearAllDeferredByMenu"/> runs.
    /// </summary>
    public class EncounterTrigger : MonoBehaviour
    {
        public UnitDefinition[] enemies;
        public string encounterName = "Forest";
        public bool consumeOnTrigger = true;

        bool _used;
        /// <summary>Set when TryStart bails only because a menu/dialog is open. Stay retries after close.</summary>
        bool _deferredByMenu;
        Collider2D _col;

        void Awake()
        {
            _col = GetComponent<Collider2D>();
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            TryStart(other, fromStay: false);
        }

        void OnTriggerStay2D(Collider2D other)
        {
            TryStart(other, fromStay: true);
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponent<PlayerWorldController>() == null)
                return;
            _deferredByMenu = false;
        }

        void FixedUpdate()
        {
            if (!_deferredByMenu || _used)
                return;
            Transform player = SaveService.FindPlayer();
            Collider2D playerCol = player != null ? player.GetComponent<Collider2D>() : null;
            if (!GeometricallyOverlaps(playerCol))
                _deferredByMenu = false;
        }

        /// <summary>
        /// Drop leftover Stay-retry flags after a teleport (F9 / <see cref="SaveService.TryApplyWorldPosition"/>).
        /// Exit2D may not fire when <c>transform.position</c> jumps.
        /// </summary>
        public static void ClearAllDeferredByMenu()
        {
            EncounterTrigger[] all = FindObjectsOfType<EncounterTrigger>();
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] != null)
                    all[i]._deferredByMenu = false;
            }
        }

        void TryStart(Collider2D other, bool fromStay)
        {
            if (_used)
                return;
            if (other.GetComponent<PlayerWorldController>() == null)
                return;

            // Transform teleports (F9) can skip Exit2D and leave stale Stay contacts.
            // Do not keep or re-arm the flag unless the player is still on this trigger.
            if (!GeometricallyOverlaps(other))
            {
                _deferredByMenu = false;
                return;
            }

            // Menu defer first so overlapping + open UI is remembered even during grace.
            if (PlayerWorldController.MenuOpen)
            {
                _deferredByMenu = true;
                return;
            }

            // Stay is only for "closed the party/dialog while still overlapping".
            // A skipped Enter during EncountersLocked must not auto-start when the lock lifts.
            if (fromStay && !_deferredByMenu)
                return;

            if (WorldBootstrap.EncountersLocked)
                return;

            UnitDefinition[] party = WorldParty.GetOrLoad();
            if (party == null || enemies == null || enemies.Length == 0)
            {
                Debug.LogError("[World] Encounter missing party or enemy data.");
                return;
            }

            _used = consumeOnTrigger;
            _deferredByMenu = false;
            EncounterContext.Begin(party, enemies, other.transform.position, encounterName);
            SaveService.Save(other.transform.position);
            Debug.Log("[World] Encounter: " + encounterName);
            SceneManager.LoadScene("Battle");
        }

        bool GeometricallyOverlaps(Collider2D other)
        {
            if (_col == null)
                _col = GetComponent<Collider2D>();
            if (_col == null || other == null)
                return false;

            var selfCircle = _col as CircleCollider2D;
            var otherCircle = other as CircleCollider2D;
            if (selfCircle != null && otherCircle != null)
            {
                Vector2 a = selfCircle.transform.TransformPoint(selfCircle.offset);
                Vector2 b = otherCircle.transform.TransformPoint(otherCircle.offset);
                float ar = selfCircle.radius * CircleScale(selfCircle.transform);
                float br = otherCircle.radius * CircleScale(otherCircle.transform);
                float r = ar + br;
                return (a - b).sqrMagnitude <= r * r;
            }

            return _col.bounds.Intersects(other.bounds);
        }

        static float CircleScale(Transform t)
        {
            Vector3 s = t.lossyScale;
            return Mathf.Max(Mathf.Abs(s.x), Mathf.Abs(s.y));
        }
    }
}
