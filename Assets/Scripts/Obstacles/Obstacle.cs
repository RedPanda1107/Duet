using UnityEngine;
using Duet.Managers;

namespace Duet.Obstacles
{
    public class Obstacle : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float fallSpeed = 2f;
        public string poolKey = "Obstacle";
        public enum Alignment
        {
            Left,
            Center,
            Right
        }

        [Header("Layout")]
        // alignment within its spawn region; default Center. Kept for future rules/visuals.
        public Alignment alignment = Alignment.Center;
        void Update()
        {
            if (GameManager.Instance.CurrentState == GameState.Playing)
            {
                transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);
            }
        }

        private void OnBecameInvisible()
        {
            // Return to pool when off screen
            PoolManager.Instance.ReturnToPool(poolKey, gameObject);
        }
    }
}
