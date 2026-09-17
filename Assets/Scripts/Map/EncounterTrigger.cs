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
    /// </summary>
    public class EncounterTrigger : MonoBehaviour
    {
        public UnitDefinition[] enemies;
        public string encounterName = "Forest";
        public bool consumeOnTrigger = true;

        bool _used;
        /// <summary>Set when TryStart bails only because a menu/dialog is open. Stay retries after close.</summary>
        bool _deferredByMenu;

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

        void TryStart(Collider2D other, bool fromStay)
        {
            if (_used)
                return;
            if (other.GetComponent<PlayerWorldController>() == null)
                return;

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
    }
}
