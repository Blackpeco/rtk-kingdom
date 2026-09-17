using UnityEngine;

namespace TsOnline
{
    /// <summary>2D top-down move with WASD / arrows. Rigidbody2D, no gravity.</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerWorldController : MonoBehaviour
    {
        public static bool MenuOpen;

        public float moveSpeed = 4.2f;
        Rigidbody2D _body;

        void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _body.gravityScale = 0f;
            _body.freezeRotation = true;
            _body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        void FixedUpdate()
        {
            if (MenuOpen)
            {
#if UNITY_6000_0_OR_NEWER
                _body.linearVelocity = Vector2.zero;
#else
                _body.velocity = Vector2.zero;
#endif
                return;
            }

            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) x = -1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) x = 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) y = -1f;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) y = 1f;
            var dir = new Vector2(x, y);
            if (dir.sqrMagnitude > 1f)
                dir.Normalize();
#if UNITY_6000_0_OR_NEWER
            _body.linearVelocity = dir * moveSpeed;
#else
            _body.velocity = dir * moveSpeed;
#endif
        }
    }
}
