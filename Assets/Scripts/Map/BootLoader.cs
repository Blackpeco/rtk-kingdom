using UnityEngine;
using UnityEngine.SceneManagement;

namespace TsOnline
{
    /// <summary>Optional Boot scene hook — jumps to World. CharacterCreate stays a stub.</summary>
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
            SceneManager.LoadScene(nextScene);
        }
    }
}
