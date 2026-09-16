using UnityEngine;
using UnityEngine.SceneManagement;

namespace TsOnline
{
    /// <summary>Overlap with the player starts a Battle using EncounterContext.</summary>
    public class EncounterTrigger : MonoBehaviour
    {
        public UnitDefinition[] enemies;
        public string encounterName = "Forest";
        public bool consumeOnTrigger = true;

        bool _used;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (_used)
                return;
            if (other.GetComponent<PlayerWorldController>() == null)
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
            EncounterContext.Begin(party, enemies, other.transform.position, encounterName);
            Debug.Log("[World] Encounter: " + encounterName);
            SceneManager.LoadScene("Battle");
        }
    }
}
