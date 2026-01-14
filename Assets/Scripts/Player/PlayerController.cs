using UnityEngine;
using Duet.Managers;
using Duet.Config;

namespace Duet.Player
{
    public class PlayerController : MonoBehaviour
    {
        private float rotateSpeed;

        private void Awake()
        {
            // Load configuration from GameConfig
            var cfg = Resources.Load<GameConfig>("GameConfig");
            rotateSpeed = cfg.rotateSpeed;
        }

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
