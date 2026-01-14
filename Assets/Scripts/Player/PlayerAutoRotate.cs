using UnityEngine;
using Duet.Managers;

namespace Duet.Player
{
    /// <summary>
    /// Simple component to rotate the player pivot clockwise while the game is in Menu state.
    /// Attach this to the PlayerPivot GameObject (or player prefab).
    /// </summary>
    public class PlayerAutoRotate : MonoBehaviour
    {
        [Tooltip("Degrees per second. Positive value rotates clockwise.")]
        public float rotationSpeed = 120f;

        private void Update()
        {
            if (GameManager.Instance == null) return;
            // Rotate while in Menu state; stop when game is Playing so player control can take over.
            if (GameManager.Instance.CurrentState == GameState.Menu)
            {
                // Rotate around Z axis clockwise: negative angle yields clockwise rotation in Unity's coordinate system
                float delta = -rotationSpeed * Time.deltaTime;
                transform.Rotate(0f, 0f, delta);
            }
        }
    }
}

