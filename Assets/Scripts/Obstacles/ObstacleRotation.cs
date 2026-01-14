using UnityEngine;

namespace Duet.Obstacles
{
    [DisallowMultipleComponent]
    public class ObstacleRotation : MonoBehaviour
    {
        private ObstacleConfig config;
        private float rotationSign = 1f;

        public void SetConfig(ObstacleConfig cfg)
        {
            config = cfg;
            // randomize direction if needed
            rotationSign = Random.value < 0.5f ? 1f : -1f;
        }

        private void Update()
        {
            if (config == null) return;
            if (!config.centerRotate) return;
            if (UnityEngine.Object.FindFirstObjectByType<Duet.Managers.GameManager>()?.CurrentState != Duet.Managers.GameState.Playing) return;

            Debug.Log($"[Rotation] '{gameObject.name}' centerRotate={config.centerRotate} rotationSpeed={config.rotationSpeed}");
            float delta = -rotationSign * config.rotationSpeed * Time.deltaTime;
            transform.Rotate(0f, 0f, delta);
        }
    }
}

