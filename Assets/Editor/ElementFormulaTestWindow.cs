using UnityEditor;
using UnityEngine;

namespace TsOnline.EditorTools
{
    /// <summary>
    /// Tools/TS Online/ — run the ข่ม/แพ้ matrix and a fixed-RNG Final damage sample.
    /// Look for PASS/FAIL lines in this window and in the Console.
    /// </summary>
    public sealed class ElementFormulaTestWindow : EditorWindow
    {
        Vector2 _scroll;
        string _report = "Click Run Tests. Results also print to the Console.";
        bool _lastFailed;

        const string MenuRun = "Tools/TS Online/Run Element Formula Tests";
        const string MenuWindow = "Tools/TS Online/Element Formula Test Window";

        [MenuItem(MenuRun, priority = 0)]
        public static void RunFromMenu()
        {
            string report = ElementFormulaSelfTest.RunAndFormat();
            bool failed = report.Contains("FAIL");
            if (failed)
                Debug.LogError(report);
            else
                Debug.Log(report);

            var window = GetWindow<ElementFormulaTestWindow>(false, "Element Formula Tests", false);
            window._report = report;
            window._lastFailed = failed;
            window.Repaint();
        }

        [MenuItem(MenuWindow, priority = 1)]
        public static void Open()
        {
            var window = GetWindow<ElementFormulaTestWindow>(false, "Element Formula Tests", true);
            window.minSize = new Vector2(520f, 360f);
            window.Show();
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("TS Online — Step 1 element / damage formula checks", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Skill ข่ม = 1.25, แพ้ = 0.80, same/opposite/none = 1.00.\n" +
                "Normal attack ข่ม = 1.12, แพ้ = 0.90, else = 1.00.\n" +
                "Heal / buff / wall / stealth skip E. Sample Final uses RNG = 1.00.",
                MessageType.Info);

            if (GUILayout.Button("Run Tests", GUILayout.Height(32f)))
                RunFromMenu();

            EditorGUILayout.Space();
            if (!string.IsNullOrEmpty(_report))
            {
                var style = new GUIStyle(EditorStyles.textArea)
                {
                    wordWrap = false,
                    richText = false
                };
                style.normal.textColor = _lastFailed ? new Color(1f, 0.45f, 0.4f) : new Color(0.45f, 0.9f, 0.5f);
                _scroll = EditorGUILayout.BeginScrollView(_scroll);
                EditorGUILayout.TextArea(_report, style, GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();
            }
        }
    }
}
