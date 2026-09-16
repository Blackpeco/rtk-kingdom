using UnityEngine;

namespace TsOnline
{
    public class WorldCameraFollow : MonoBehaviour
    {
        public Transform target;
        public float lerp = 8f;

        void LateUpdate()
        {
            if (target == null)
                return;
            Vector3 p = transform.position;
            Vector3 goal = new Vector3(target.position.x, target.position.y, p.z);
            transform.position = Vector3.Lerp(p, goal, Time.deltaTime * lerp);
        }
    }
}
