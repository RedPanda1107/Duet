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
            Vector3 targetPos = init != null ? init.playerStartPosition : Vector3.zero;
            // Read transition parameters from GameConfig if available
            var cfg = UnityEngine.Resources.Load<Duet.Config.GameConfig>("GameConfig");
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
                PoolManager.Instance?.ReturnToPool(o);
            }
            // Reset player position if present
            var player = GameObject.Find("PlayerPivot");
            if (player != null)
            {
                var init = UnityEngine.Object.FindFirstObjectByType<GameInitializer>();
                if (init != null) player.transform.position = init.playerStartPosition;
            }

            // Start playing
            SetGameState(GameState.Playing);
            UIManager.Instance?.HideGameOver();
            UIManager.Instance?.ShowGameHUD();
            UnityEngine.Object.FindFirstObjectByType<GameInitializer>()?.OnStartGame();
        }

        public void ReturnToMenu()
        {
            // Switch to menu state without loading scenes
            SetGameState(GameState.Menu);
            // Stop spawning
            var spawner = UnityEngine.Object.FindFirstObjectByType<Duet.Obstacles.ObstacleSpawner>();
            if (spawner != null) spawner.enabled = false;
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
