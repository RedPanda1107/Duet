using UnityEngine;
using Duet.Managers;

namespace Duet.Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Rotation Settings")]
        public float rotateSpeed = 180f; // degrees per second

        void Update()
        {
            if (GameManager.Instance.CurrentState == GameState.Playing)
            {
                int direction = InputManager.Instance.GetRotationDirection();
                transform.Rotate(0, 0, -direction * rotateSpeed * Time.deltaTime);
            }
        }
    }
}
