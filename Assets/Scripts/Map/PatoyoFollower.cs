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
            sr.sprite = WorldArt.MakeShape(new Color(1f, 0.52f, 0.70f), 24, WorldArt.Shape.Blob, true,
                WorldArt.ActorMark.Patoyo);
            sr.sortingOrder = 6;
            WorldArt.AttachShadow(go.transform, 6, 1.1f);
            var follow = go.AddComponent<PatoyoFollower>();
            follow.target = lead;
            WorldArt.MakeLabel(go.transform, "ปาโต้เยา", new Vector3(0f, 0.62f, 0f),
                new Color(1f, 0.86f, 0.92f), 0.12f, 22);
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
