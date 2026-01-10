using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Duet.Managers
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Menu UI")]
        // Optional: assign the whole menu root (e.g. "Meun" GameObject) so ShowMenu hides entire menu.
        public GameObject menuRoot;
        public Button startButton;
        public Button quitButton;

        [Header("Game UI")]
        public GameObject gameHUD;
        public GameObject gameOverPanel;
        public TextMeshProUGUI gameOverText;
        public Button restartButton;
        public Button menuButton;
        public Button pauseButton;
        public Button continueButton;
        public TextMeshProUGUI scoreText;

        // score is managed by ScoreManager

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            // UIManager is scene-local (do not persist across scenes) so do not call DontDestroyOnLoad here.
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            SetupButtonListeners();
            UpdateScoreDisplay();
        }

        private void SetupButtonListeners()
        {
            if (startButton != null)
                startButton.onClick.AddListener(() =>
                {
                    AudioManager.Instance.PlayClickSound();
                    GameManager.Instance.StartGame();
                });

            if (quitButton != null)
                quitButton.onClick.AddListener(() =>
                {
                    AudioManager.Instance.PlayClickSound();
                    GameManager.Instance.QuitGame();
                });

            if (restartButton != null)
                restartButton.onClick.AddListener(() =>
                {
                    AudioManager.Instance.PlayClickSound();
                    GameManager.Instance.RestartGame();
                });

            if (menuButton != null)
                menuButton.onClick.AddListener(() =>
                {
                    AudioManager.Instance.PlayClickSound();
                    GameManager.Instance.ReturnToMenu();
                });
            
            if (pauseButton != null)
                pauseButton.onClick.AddListener(() =>
                {
                    AudioManager.Instance.PlayClickSound();
                    GameManager.Instance.PauseGame();
                });
            
            if (continueButton != null)
                continueButton.onClick.AddListener(() =>
                {
                    AudioManager.Instance.PlayClickSound();
                    GameManager.Instance.ResumeGame();
                });
        }

        public void ShowGameOver(string message = "Game Over", bool showContinue = false)
        {
            if (gameOverText != null)
        {
                gameOverText.text = message;
            }

            if (gameOverPanel != null)
                gameOverPanel.SetActive(true);

            if (gameHUD != null)
                gameHUD.SetActive(false);

            if (continueButton != null)
                continueButton.gameObject.SetActive(showContinue);
        }

        public void HideGameOver()
        {
            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);

            if (gameHUD != null)
                gameHUD.SetActive(true);
        }

        // Called when entering game scene
        public void OnGameSceneLoaded()
        {
            // Force hide game over panel and show game HUD
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
                Debug.Log("GameOver panel hidden");
            }

            if (gameHUD != null)
            {
                gameHUD.SetActive(true);
                Debug.Log("Game HUD shown");
            }
            else
            {
                Debug.LogWarning("GameHUD is null in UIManager");
            }
            // Reset score when entering game scene
            if (ScoreManager.Instance != null) ScoreManager.Instance.ResetScore();
            UpdateScoreDisplay();
        }

        // Called when entering menu scene
        public void OnMenuSceneLoaded()
        {
            // Menu specific setup if needed
        }
        private void OnEnable()
        {
            if (ScoreManager.Instance != null)
                ScoreManager.Instance.OnScoreChanged += OnScoreChanged;
        }

        private void OnDisable()
        {
            if (ScoreManager.Instance != null)
                ScoreManager.Instance.OnScoreChanged -= OnScoreChanged;
        }

        private void OnScoreChanged(int newScore)
        {
            UpdateScoreDisplay();
        }

        private void UpdateScoreDisplay()
        {
            if (scoreText != null)
            {
                int s = ScoreManager.Instance != null ? ScoreManager.Instance.GetScore() : 0;
                scoreText.text = $"Score: {s}";
            }
        }
        
        // Show the main menu UI and hide game HUD / game over
        public void ShowMenu()
        {
            // Prefer toggling the whole menu root. If not assigned, do not attempt per-button toggles.
            if (menuRoot != null)
            {
                menuRoot.SetActive(true);
            }

            if (gameHUD != null) gameHUD.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
        }

        // Hide only the menu root (do not toggle HUD)
        public void HideMenu()
        {
            if (menuRoot != null)
            {
                menuRoot.SetActive(false);
            }
            else
            {
                Debug.LogWarning("[UIManager] menuRoot is not assigned; cannot HideMenu()");
            }
        }

        // Show the in-game HUD and hide menu / game over
        public void ShowGameHUD()
        {
            // Prefer hiding the whole menu root. If not assigned, do not attempt per-button toggles.
            if (menuRoot != null)
            {
                menuRoot.SetActive(false);
            }

            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (gameHUD != null) gameHUD.SetActive(true);
        }
    }
}
