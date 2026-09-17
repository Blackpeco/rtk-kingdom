using UnityEngine;
using UnityEngine.SceneManagement;

namespace TsOnline
{
    /// <summary>Boot: existing save → World; otherwise CharacterCreate.</summary>
    public class BootLoader : MonoBehaviour
    {
        public string nextScene = "World";
        public float delay = 0.4f;

        void Start()
        {
            Invoke(nameof(Go), delay);
        }

        void Go()
        {
            if (SaveService.Exists())
            {
                PartyManager.Ensure();
                if (SaveService.TryLoad())
                {
                    SceneManager.LoadScene(string.IsNullOrEmpty(nextScene) ? "World" : nextScene);
                    return;
                }
            }

            SceneManager.LoadScene("CharacterCreate");
        }
    }
}
