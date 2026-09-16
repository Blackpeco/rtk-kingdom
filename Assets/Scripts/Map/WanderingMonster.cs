using UnityEngine;

namespace TsOnline
{
    public class WanderingMonster : MonoBehaviour
    {
        public Vector3 origin;
        public float range = 1.4f;
        public float speed = 1.1f;
        float _phase;

        void Start()
        {
            origin = transform.position;
            _phase = Random.Range(0f, Mathf.PI * 2f);
        }

        void Update()
        {
            _phase += Time.deltaTime * speed;
            transform.position = origin + new Vector3(Mathf.Cos(_phase), Mathf.Sin(_phase * 0.7f), 0f) * range;
        }
    }
}
