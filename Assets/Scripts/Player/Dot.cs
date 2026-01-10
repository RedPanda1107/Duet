using UnityEngine;
using Duet.Managers;

namespace Duet.Player
{
    public class Dot : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other == null) return;
            if (other.CompareTag("Obstacle"))
            {
                Debug.Log($"[Dot] Hit obstacle: {other.gameObject.name}");
                GameManager.Instance.OnPlayerHit();
            }
        }
    }
}
