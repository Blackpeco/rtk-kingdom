using UnityEngine;

namespace TsOnline
{
    /// <summary>ปาโต้เยา — round mascot that lerps behind the World lead. Not a party slot.</summary>
    public class PatoyoFollower : MonoBehaviour
    {
        public Transform target;
        public float followDistance = 0.9f;
        public float lerp = 5.5f;

        public static GameObject Spawn(Transform lead)
        {
            var go = new GameObject("Patoyo");
            Vector3 start = lead != null
                ? lead.position + new Vector3(-0.85f, -0.4f, 0f)
                : new Vector3(-7f, -0.4f, 0f);
            go.transform.position = start;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = WorldArt.MakeQuad(new Color(1f, 0.58f, 0.72f), 12);
            sr.sortingOrder = 6;
            var follow = go.AddComponent<PatoyoFollower>();
            follow.target = lead;

            var label = new GameObject("Name");
            label.transform.SetParent(go.transform, false);
            label.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            var tm = label.AddComponent<TextMesh>();
            tm.text = "ปาโต้เยา";
            tm.characterSize = 0.12f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.fontSize = 22;
            tm.color = new Color(1f, 0.85f, 0.9f);
            return go;
        }

        void LateUpdate()
        {
            if (target == null)
                return;
            Vector3 dest = target.position + new Vector3(-followDistance, -0.38f, 0f);
            float t = 1f - Mathf.Exp(-lerp * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, dest, t);
        }
    }
}
