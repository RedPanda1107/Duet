using UnityEngine;
using UnityEngine.SceneManagement;
using System;

namespace Duet.Managers
{
    public enum GameState
    {
        Menu,
        Playing,
        Paused,
        GameOver
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState CurrentState { get; private set; }

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
            }
        }

        private void Start()
        {
            // Initialize to menu state
            SetGameState(GameState.Menu);
        }

        public void StartGame()
        {
            // Start a player transition (move/rotate/scale) if PlayerPivot has PlayerTransition.
            var playerObj = GameObject.Find("PlayerPivot");
            var init = UnityEngine.Object.FindFirstObjectByType<GameInitializer>();
            var cfg = UnityEngine.Resources.Load<Duet.Config.GameConfig>("GameConfig");
            Vector3 targetPos = cfg != null ? cfg.playerStartPosition : Vector3.zero;
            // Read transition parameters from GameConfig if available
            float transitionDuration = cfg != null ? cfg.transitionDuration : 1f;
            float targetScale = cfg != null ? cfg.transitionTargetScale : 80f; // final uniform scale per design

            // Hide the menu root immediately when Start is clicked
            UIManager.Instance?.HideMenu();

            if (playerObj != null)
            {
                // Avoid compile-time dependency on PlayerTransition type; use reflection to call StartTransition if present.
                var comp = playerObj.GetComponent("PlayerTransition");
                if (comp != null)
                {
                    var type = comp.GetType();
                    var method = type.GetMethod("StartTransition", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (method != null)
                    {
                        Action onComplete = () =>
                        {
                            SetGameState(GameState.Playing);
                            UnityEngine.Object.FindFirstObjectByType<GameInitializer>()?.OnStartGame();
                            UIManager.Instance?.ShowGameHUD();
                        };
                        method.Invoke(comp, new object[] { targetPos, transitionDuration, targetScale, onComplete });
                        return;
                    }
                }
            }

            // If no transition component present, do nothing (no fallback).
        }

        public void OnPlayerHit()
        {
            SetGameState(GameState.GameOver);
            AudioManager.Instance?.PlayCollisionGameOver();
            // Stop spawner and notify initializer
            var init = UnityEngine.Object.FindFirstObjectByType<GameInitializer>();
            init?.OnGameOver();
            UIManager.Instance?.ShowGameOver();
        }

        public void PauseGame()
        {
            SetGameState(GameState.Paused);
            // Stop spawning
            var spawner = UnityEngine.Object.FindFirstObjectByType<Duet.Obstacles.ObstacleSpawner>();
            if (spawner != null) spawner.enabled = false;
            // Pause time
            Time.timeScale = 0f;
            // Disable player input controller if present
            var player = GameObject.Find("PlayerPivot");
            if (player != null)
            {
                var torque = player.GetComponent<Duet.Player.PlayerTorqueController>();
                if (torque != null) torque.enabled = false;
            }
            // Show paused overlay using existing GameOver panel with custom text, show Continue button
            UIManager.Instance?.ShowGameOver("PAUSED", true);
        }

        public void ResumeGame()
        {
            // Resume time
            Time.timeScale = 1f;
            // Restore spawning
            var spawner = UnityEngine.Object.FindFirstObjectByType<Duet.Obstacles.ObstacleSpawner>();
            if (spawner != null) spawner.enabled = true;
            // Re-enable player input controller if present
            var player = GameObject.Find("PlayerPivot");
            if (player != null)
            {
                var torque = player.GetComponent<Duet.Player.PlayerTorqueController>();
                if (torque != null) torque.enabled = true;
            }
            // Hide paused overlay and show HUD
            UIManager.Instance?.HideGameOver();
            SetGameState(GameState.Playing);
        }

        public void RestartGame()
        {
            // Reset score
            ScoreManager.Instance?.ResetScore();
            // Return all active obstacles to pool
            var obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
            foreach (var o in obstacles)
            {
                var obstacleComp = o.GetComponent<Duet.Obstacles.Obstacle>();
                if (obstacleComp != null && !string.IsNullOrEmpty(obstacleComp.poolKey))
                {
                    PoolManager.Instance?.ReturnToPool(obstacleComp.poolKey, o);
                }
            }
            // Reset player position if present
            var player = GameObject.Find("PlayerPivot");
            if (player != null)
            {
                var cfg = UnityEngine.Resources.Load<Duet.Config.GameConfig>("GameConfig");
                if (cfg != null) player.transform.position = cfg.playerStartPosition;
            }

            // Start playing
            SetGameState(GameState.Playing);
            UIManager.Instance?.HideGameOver();
            UIManager.Instance?.ShowGameHUD();
            UnityEngine.Object.FindFirstObjectByType<GameInitializer>()?.OnStartGame();
        }

        public void ReturnToMenu()
        {
            // Start reverse transition back to menu
            var playerObj = GameObject.Find("PlayerPivot");
            var cfg = UnityEngine.Resources.Load<Duet.Config.GameConfig>("GameConfig");

            if (playerObj != null && cfg != null)
            {
                // If the game is currently paused (timeScale == 0), resume time so transition coroutines run.
                // PauseGame sets Time.timeScale = 0 which would stop Update()/coroutine progress that uses Time.deltaTime.
                if (Time.timeScale == 0f)
                {
                    Time.timeScale = 1f;
                    Debug.Log("[GameManager] Time was paused; resuming time to perform return-to-menu transition.");
                }

                // Use reflection to call StartReturnToMenuTransition if PlayerTransition component exists
                var comp = playerObj.GetComponent("PlayerTransition");
                if (comp != null)
                {
                    var type = comp.GetType();
                    var method = type.GetMethod("StartReturnToMenuTransition", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (method != null)
                    {
                        Action onComplete = () =>
                        {
                            // Return all active obstacles to pool (same as RestartGame)
                            var obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
                            foreach (var o in obstacles)
                            {
                                var obstacleComp = o.GetComponent<Duet.Obstacles.Obstacle>();
                                if (obstacleComp != null && !string.IsNullOrEmpty(obstacleComp.poolKey))
                                {
                                    PoolManager.Instance?.ReturnToPool(obstacleComp.poolKey, o);
                                }
                            }

                            // Switch to menu state after transition completes
                            SetGameState(GameState.Menu);
                            // Stop spawning
                            var spawner = UnityEngine.Object.FindFirstObjectByType<Duet.Obstacles.ObstacleSpawner>();
                            if (spawner != null) spawner.enabled = false;
                            UIManager.Instance?.ShowMenu();
                        };

                        method.Invoke(comp, new object[] {
                            cfg.menuPlayerPosition,   // Return to menu position
                            0f,                       // Return to 0 degrees, then PlayerAutoRotate takes over
                            150f,                     // Force return scale to 150
                            cfg.transitionDuration,   // Use same duration
                            onComplete
                        });
                        return;
                    }
                }
            }

            // Fallback: directly switch to menu state if transition component not found
            SetGameState(GameState.Menu);
            var spawnerFallback = UnityEngine.Object.FindFirstObjectByType<Duet.Obstacles.ObstacleSpawner>();
            if (spawnerFallback != null) spawnerFallback.enabled = false;
            UIManager.Instance?.ShowMenu();
        }

        public void SetGameStateToPlaying()
        {
            SetGameState(GameState.Playing);
        }

        public void QuitGame()
        {
            Application.Quit();
        }

        private void SetGameState(GameState newState)
        {
            CurrentState = newState;
            Debug.Log($"Game state changed to: {newState}");
        }
    }
}
