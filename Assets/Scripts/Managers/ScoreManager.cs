using UnityEngine;

namespace Duet.Managers
{
    /// <summary>
    /// Central score manager / observer.
    /// Subscribes to PoolManager.OnReturnedToPool and updates score.
    /// Exposes OnScoreChanged(int) event for UI or other systems to observe.
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        private int score;

        public event System.Action<int> OnScoreChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void OnEnable()
        {
            if (PoolManager.Instance != null)
                PoolManager.Instance.OnReturnedToPool += HandleReturnedToPool;
        }

        private void Start()
        {
            // In case PoolManager wasn't available in OnEnable
            if (PoolManager.Instance != null)
            {
                PoolManager.Instance.OnReturnedToPool -= HandleReturnedToPool;
                PoolManager.Instance.OnReturnedToPool += HandleReturnedToPool;
            }
        }

        private void OnDisable()
        {
            if (PoolManager.Instance != null)
                PoolManager.Instance.OnReturnedToPool -= HandleReturnedToPool;
        }

        private void HandleReturnedToPool(string key, GameObject obj)
        {
            if (key == "Obstacle")
            {
                AddScore(1);
            }
        }

        public void AddScore(int delta)
        {
            score += delta;
            OnScoreChanged?.Invoke(score);
        }

        public int GetScore() => score;

        public void ResetScore()
        {
            score = 0;
            OnScoreChanged?.Invoke(score);
        }
    }
}


