using UnityEngine;
using Duet.Managers;
using Duet.Config;

namespace Duet.Player
{
    /// <summary>
    /// Rigidbody2D-based controller that applies torque for rotation,
    /// giving the player a feeling of inertia. Attach to the PlayerPivot root.
    /// </summary>
    public class PlayerTorqueController : MonoBehaviour
    {
        private float torqueForce;
        private float maxAngularVelocity;
        private float angularDrag;

        private Rigidbody2D rb;

        private void Awake()
        {
            // Load configuration from GameConfig
            var cfg = Resources.Load<GameConfig>("GameConfig");
            torqueForce = cfg.torqueForce;
            maxAngularVelocity = cfg.maxAngularVelocity;
            angularDrag = cfg.angularDrag;

            rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
            }

            // Configure Rigidbody2D for pivot-only rotation:
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezePosition;
            rb.angularDrag = angularDrag;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        private void FixedUpdate()
        {
            // Only respond while game is playing
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
                return;

            int dir = 0;
            if (InputManager.Instance != null)
                dir = InputManager.Instance.GetRotationDirection(); // -1 left, 0 none, +1 right
            else
            {
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) dir = -1;
                if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) dir = 1;
            }

            if (dir != 0)
            {
                // Input returns -1 for left; apply torque such that left input produces CCW rotation.
                float applied = -dir * torqueForce;
                rb.AddTorque(applied);
            }

            // Clamp angular velocity to avoid runaway spinning
            rb.angularVelocity = Mathf.Clamp(rb.angularVelocity, -maxAngularVelocity, maxAngularVelocity);
        }
    }
}


