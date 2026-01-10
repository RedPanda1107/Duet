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
    public float dotDistance = 2f;
    [Header("Player Transform")]
    public Vector3 playerStartPosition = new Vector3(0f, -4f, 0f);

    [Header("Obstacle Setup")]
    public GameObject obstaclePrefab;

    [Header("Spawner Setup")]
    public ObstacleSpawner spawner;

    private void Awake()
    {
        // Load game configuration if present (Assets/Create -> Duet/Game Config)
        var cfg = Resources.Load<GameConfig>("GameConfig");
        if (cfg != null)
        {
            // override inspector defaults with config
            playerStartPosition = cfg.playerStartPosition;
            dotDistance = cfg.dotDistance;
        }

        
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
        SetupObstaclePool();
        // Reset score and enable spawner (assumes spawner assigned in Inspector)
        ScoreManager.Instance?.ResetScore();
        spawner.enabled = true;
        // Reposition player (assumes PlayerPivot exists)
        GameObject.Find("PlayerPivot").transform.position = playerStartPosition;
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
            // Place prefab at configured start position and reset rotation
            instantiated.transform.position = playerStartPosition;
            instantiated.transform.rotation = Quaternion.identity;
            // Scale down instantiated player to configured scale
            var cfg = Resources.Load<GameConfig>("GameConfig");
            float scale = cfg != null ? cfg.playerScale : 0.5f;
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
        var cfg = Resources.Load<GameConfig>("GameConfig");
        int initial = cfg != null ? cfg.obstaclePoolInitialCount : 20;
        PoolManager.Instance.RegisterPrefab("Obstacle", obstaclePrefab, initial);
    }

    private void SetupSpawner()
    {
        // Spawner must be assigned in the scene; no automatic creation.
    }
}
