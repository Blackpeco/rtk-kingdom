using UnityEngine;
using UnityEngine.SceneManagement;

namespace TsOnline
{
    /// <summary>
    /// Overlap with the player starts a Battle using EncounterContext.
    /// Skips while <see cref="PlayerWorldController.MenuOpen"/> (party panel or ยายเมือง dialog).
    /// Wanderers may still roam; Stay retries after the menu closes.
    /// </summary>
    public class EncounterTrigger : MonoBehaviour
    {
        public UnitDefinition[] enemies;
        public string encounterName = "Forest";
        public bool consumeOnTrigger = true;

        bool _used;

        void OnTriggerEnter2D(Collider2D other)
        {
            TryStart(other);
        }

        void OnTriggerStay2D(Collider2D other)
        {
            TryStart(other);
        }

        void TryStart(Collider2D other)
        {
            if (_used)
                return;
            if (other.GetComponent<PlayerWorldController>() == null)
                return;
            if (WorldBootstrap.EncountersLocked)
                return;
            if (PlayerWorldController.MenuOpen)
                return;

            UnitDefinition[] party = WorldParty.GetOrLoad();
            if (party == null || enemies == null || enemies.Length == 0)
            {
                Debug.LogError("[World] Encounter missing party or enemy data.");
                return;
            }

            _used = consumeOnTrigger;
            EncounterContext.Begin(party, enemies, other.transform.position, encounterName);
            SaveService.Save(other.transform.position);
            Debug.Log("[World] Encounter: " + encounterName);
            SceneManager.LoadScene("Battle");
        }
    }
}
