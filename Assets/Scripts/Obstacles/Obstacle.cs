using UnityEngine;
using Duet.Managers;
using Duet.Config;

namespace Duet.Obstacles
{
    public class Obstacle : MonoBehaviour
    {
        private float fallSpeed;
        public string poolKey;
        public enum Alignment
        {
            Left,
            Center,
            Right
        }

        [Header("Layout")]
        // alignment within its spawn region; default Center. Kept for future rules/visuals.
        public Alignment alignment = Alignment.Center;
        // reference to runtime config (may be null)
        public ObstacleConfig config;

        private ObstacleMovement movementComp;
        private ObstacleRotation rotationComp;

        private void Awake()
        {
            // Load configuration from GameConfig
            var cfg = Resources.Load<GameConfig>("GameConfig");
            fallSpeed = cfg.obstacleFallSpeed;

            // Ensure movement and rotation components exist
            movementComp = GetComponent<ObstacleMovement>();
            if (movementComp == null) movementComp = gameObject.AddComponent<ObstacleMovement>();
            rotationComp = GetComponent<ObstacleRotation>();
            if (rotationComp == null) rotationComp = gameObject.AddComponent<ObstacleRotation>();
        }

        public void ApplyConfig(ObstacleConfig cfg)
        {
            config = cfg;
            // pass config to subcomponents
            movementComp.SetConfig(cfg, fallSpeed);
            rotationComp.SetConfig(cfg);
            string cfgName = "null";
            if (cfg != null)
            {
                if (!string.IsNullOrEmpty(cfg.id)) cfgName = cfg.id;
                else if (cfg.prefab != null) cfgName = cfg.prefab.name;
                else cfgName = "(no-prefab)";
            }
            Debug.Log($"[Obstacle] ApplyConfig on '{gameObject.name}' cfg={cfgName}, fallSpeed={fallSpeed}");
        }

        private void OnBecameInvisible()
        {
            // Return to pool when off screen (only if game is playing to avoid menu/preview recycling)
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
            {
                PoolManager.Instance.ReturnToPool(poolKey, gameObject);
            }
        }
    }
}
