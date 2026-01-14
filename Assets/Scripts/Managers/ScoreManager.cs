using UnityEngine;
using System.IO;

namespace Duet.Managers
{
    [System.Serializable]
    public class GameRecord
    {
        public int highScore = 0;
    }

    /// <summary>
    /// Central score manager / observer.
    /// Subscribes to PoolManager.OnReturnedToPool and updates score.
    /// Exposes OnScoreChanged(int) event for UI or other systems to observe.
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        private int score;
        private GameRecord gameRecord;
        private string recordFilePath;

        public event System.Action<int> OnScoreChanged;
        public event System.Action<int> OnHighScoreChanged;

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

            // Initialize record file path
            recordFilePath = Path.Combine(Application.persistentDataPath, "gameRecord.json");
            LoadGameRecord();
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
            // Check if the returned object has an Obstacle component
            if (obj != null && obj.GetComponent<Duet.Obstacles.Obstacle>() != null)
            {
                Debug.Log($"[ScoreManager] Obstacle returned to pool, increasing score");
                AddScore(1);
            }
        }

        public void AddScore(int delta)
        {
            score += delta;
            Debug.Log($"[ScoreManager] Score changed: {score - delta} -> {score}");
            OnScoreChanged?.Invoke(score);

            // Check if this is a new high score
            if (score > gameRecord.highScore)
            {
                gameRecord.highScore = score;
                SaveGameRecord();
                OnHighScoreChanged?.Invoke(gameRecord.highScore);
                Debug.Log($"[ScoreManager] New high score: {gameRecord.highScore}");
            }
        }

        public int GetScore() => score;

        public int GetHighScore() => gameRecord.highScore;

        public void ResetScore()
        {
            score = 0;
            OnScoreChanged?.Invoke(score);
        }

        private void LoadGameRecord()
        {
            try
            {
                if (File.Exists(recordFilePath))
                {
                    string json = File.ReadAllText(recordFilePath);
                    gameRecord = JsonUtility.FromJson<GameRecord>(json);
                    Debug.Log($"[ScoreManager] Loaded high score: {gameRecord.highScore}");
                }
                else
                {
                    gameRecord = new GameRecord();
                    Debug.Log("[ScoreManager] Created new game record with high score: 0");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ScoreManager] Failed to load game record: {e.Message}");
                gameRecord = new GameRecord();
            }
        }

        private void SaveGameRecord()
        {
            try
            {
                string json = JsonUtility.ToJson(gameRecord, true);
                File.WriteAllText(recordFilePath, json);
                Debug.Log($"[ScoreManager] Saved high score: {gameRecord.highScore}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ScoreManager] Failed to save game record: {e.Message}");
            }
        }
    }
}


