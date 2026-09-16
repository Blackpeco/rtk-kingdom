using UnityEngine;

namespace TsOnline
{
    /// <summary>
    /// Drop on any GameObject (Battle scene recommended). Logs the same formula checks as the Editor window.
    /// </summary>
    public class DamageFormulaSmokeTest : MonoBehaviour
    {
        [Tooltip("Run checks when entering Play Mode.")]
        public bool runOnStart = true;

        [Tooltip("Run checks when the inspector value changes in Edit Mode.")]
        public bool runOnValidate = true;

        void Start()
        {
            if (runOnStart)
                Run();
        }

        void OnValidate()
        {
            if (!runOnValidate)
                return;
            Run();
        }

        [ContextMenu("Run Element Formula Checks")]
        public void Run()
        {
            string report = ElementFormulaSelfTest.RunAndFormat();
            if (report.Contains("FAIL"))
                Debug.LogError(report, this);
            else
                Debug.Log(report, this);
        }
    }
}
