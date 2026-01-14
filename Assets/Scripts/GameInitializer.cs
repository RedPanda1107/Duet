using UnityEngine;
using UnityEngine.SceneManagement;
using Duet.Managers;
using Duet.Player;
using Duet.Obstacles;
using Duet.Config;

public class GameInitializer : MonoBehaviour
{
    [Header("Player Setup")]
    public GameObject dotPrefab;
    public GameObject playerPrefab; // optional: if set, instantiate this instead of creating pivot+dots
    public float dotRadius = 1f;
    [Header("Player Transform")]
    // playerStartPosition and dotDistance are sourced from GameConfig; do not set here.
    private Duet.Config.GameConfig gameConfig;

    [Header("Obstacle Setup")]
    [Tooltip("Optional: assign ObstacleConfig assets here. If empty, configs will be loaded from Resources/Obstacles at runtime.")]
    public ObstacleConfig[] obstacleConfigs;

    [Header("Spawner Setup")]
    public ObstacleSpawner spawner;

    private void Awake()
    {
        // Load game configuration if present (Assets/Create -> Duet/Game Config)
        gameConfig = Resources.Load<GameConfig>("GameConfig");

        
    }

    private void Start()
    {
        // Single-scene mode: always initialize managers, show menu and keep spawner disabled until game starts
            SetupMenuScene();
        if (spawner != null) spawner.enabled = false;
    }

   

    private void SetupGameScene()
    {
        CreatePlayer();
        SetupObstaclePool();
        SetupSpawner();

        // Set game state to playing after everything is set up
        GameManager.Instance.SetGameStateToPlaying();
        UIManager.Instance.OnGameSceneLoaded();

        Debug.Log("Game scene setup completed");
    }

    private void SetupMenuScene()
    {
        UIManager.Instance.OnMenuSceneLoaded();
        // Show menu UI by default
        UIManager.Instance.ShowMenu();
    }

    // Called by GameManager when starting gameplay
    public void OnStartGame()
    {
        // Determine config list: prefer inspector-assigned list, otherwise load from Resources/Obstacles
        ObstacleConfig[] configsToRegister = null;
        if (obstacleConfigs != null && obstacleConfigs.Length > 0)
        {
            configsToRegister = obstacleConfigs;
        }
        else
        {
            configsToRegister = Resources.LoadAll<ObstacleConfig>("Obstacles");
        }

        var cfgList = new System.Collections.Generic.List<ObstacleConfig>();
        foreach (var cfg in configsToRegister)
        {
            string key = !string.IsNullOrEmpty(cfg.id) ? cfg.id : (cfg.prefab != null ? cfg.prefab.name : "Obstacle");
            int initial = Resources.Load<GameConfig>("GameConfig")?.obstaclePoolInitialCount ?? 20;
            PoolManager.Instance.RegisterPrefab(key, cfg.prefab, initial);
            cfgList.Add(cfg);
        }

        // assign configs to spawner (assumes spawner is assigned in inspector)
        spawner.obstacleConfigs = cfgList.ToArray();

        // Reset score and enable spawner (assumes spawner assigned in Inspector)
        ScoreManager.Instance?.ResetScore();
        spawner.enabled = true;
        // Reposition player (assumes PlayerPivot exists)
        if (gameConfig != null)
            GameObject.Find("PlayerPivot").transform.position = gameConfig.playerStartPosition;
        else
            GameObject.Find("PlayerPivot").transform.position = new Vector3(0f, -4f, 0f);
    }

    // Called by GameManager when game over occurs
    public void OnGameOver()
    {
        if (spawner != null) spawner.enabled = false;
    }

    private void CreatePlayer()
    {
        // If a player prefab is provided, instantiate it (assumed to contain pivot + dots)
        if (playerPrefab != null)
        {
            GameObject instantiated = Instantiate(playerPrefab);
            instantiated.name = "PlayerPivot";
            // Place prefab at menu position and reset rotation
            instantiated.transform.position = gameConfig != null ? gameConfig.menuPlayerPosition : new Vector3(0f, 0f, 0f);
            instantiated.transform.rotation = Quaternion.identity;
            // Scale player to menu scale
            var cfg = Resources.Load<GameConfig>("GameConfig");
            float scale = cfg != null ? cfg.menuPlayerScale : 1.0f;
            instantiated.transform.localScale = Vector3.one * scale;
        }
    }

    private void SetupDot(GameObject dot)
    {
        dot.AddComponent<Dot>();

        // Add Rigidbody2D
        Rigidbody2D rb = dot.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        // Add CircleCollider2D
        CircleCollider2D collider = dot.AddComponent<CircleCollider2D>();
        collider.radius = dotRadius;
            // Use trigger collisions to reliably detect hits with obstacles
            collider.isTrigger = true;
        // Ensure tag is Player
        dot.tag = "Player";
    }

    private void SetupObstaclePool()
    {
        // Deprecated: obstacle prefab registration via single prefab removed in favor of ObstacleConfig-based registration.
        // Kept empty to avoid compile errors for any legacy calls.
    }

    private void SetupSpawner()
    {
        // Spawner must be assigned in the scene; no automatic creation.
    }
}
