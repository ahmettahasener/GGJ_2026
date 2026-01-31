using UnityEngine;
using UnityEngine.UI;
using GGJ_2026.Managers;
using UnityEngine.SceneManagement;

namespace GGJ_2026.UI
{
    public class GameUI : MonoBehaviour
    {
        public static GameUI Instance { get; private set; }

        [Header("Panels")]
        [SerializeField] private GameObject _mainMenuPanel;
        [SerializeField] private GameObject _tutorialPanel;
        [SerializeField] private GameObject _gameOverPanel;

        [Header("Main Menu Buttons")]
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _tutorialButton;
        [SerializeField] private Button _quitButton;

        [Header("Tutorial Buttons")]
        [SerializeField] private Button _closeTutorialButton;

        [Header("Game Over Buttons")]
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _quitFromGameOverButton;

        private Player.PlayerMovement _playerMovement;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // Find Player
            _playerMovement = FindObjectOfType<Player.PlayerMovement>();

            // Setup Button Listeners
            SetupButtons();
        }

        private void Start()
        {
            // Subscribe to Game State Changes
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged += HandleGameStateChanged;
            }

            // Show Main Menu at start
            ShowMainMenu();
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged -= HandleGameStateChanged;
            }
        }

        private void SetupButtons()
        {
            // Main Menu
            if (_playButton != null)
                _playButton.onClick.AddListener(OnPlayClicked);
            if (_tutorialButton != null)
                _tutorialButton.onClick.AddListener(OnTutorialClicked);
            if (_quitButton != null)
                _quitButton.onClick.AddListener(OnQuitClicked);

            // Tutorial
            if (_closeTutorialButton != null)
                _closeTutorialButton.onClick.AddListener(OnCloseTutorialClicked);

            // Game Over
            if (_restartButton != null)
                _restartButton.onClick.AddListener(OnRestartClicked);
            if (_quitFromGameOverButton != null)
                _quitFromGameOverButton.onClick.AddListener(OnQuitClicked);
        }

        private void HandleGameStateChanged(GameState newState)
        {
            switch (newState)
            {
                case GameState.GameOver:
                    ShowGameOver();
                    break;
                case GameState.GameWin:
                    ShowGameWin();
                    break;
            }
        }

        #region Main Menu
        private void ShowMainMenu()
        {
            SetPanelActive(_mainMenuPanel, true);
            SetPanelActive(_tutorialPanel, false);
            SetPanelActive(_gameOverPanel, false);

            SetPlayerControl(false);
            UnlockCursor();
        }

        private void OnPlayClicked()
        {
            Debug.Log("Play Button Clicked");

            SetPanelActive(_mainMenuPanel, false);
            SetPlayerControl(true);

            // Start the game
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(GameState.MaskSelection);
            }
        }

        private void OnTutorialClicked()
        {
            Debug.Log("Tutorial Button Clicked");
            SetPanelActive(_tutorialPanel, true);
        }

        private void OnCloseTutorialClicked()
        {
            Debug.Log("Close Tutorial Clicked");
            SetPanelActive(_tutorialPanel, false);
        }

        private void OnQuitClicked()
        {
            Debug.Log("Quit Button Clicked");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
        }
        #endregion

        #region Game Over
        private void ShowGameOver()
        {
            Debug.Log("Showing Game Over Panel");

            SetPanelActive(_mainMenuPanel, false);
            SetPanelActive(_tutorialPanel, false);
            SetPanelActive(_gameOverPanel, true);

            SetPlayerControl(false);
            UnlockCursor();
        }

        private void ShowGameWin()
        {
            Debug.Log("Showing Game Win Panel");

            // You can create a separate Win Panel or reuse GameOver with different text
            SetPanelActive(_mainMenuPanel, false);
            SetPanelActive(_tutorialPanel, false);
            SetPanelActive(_gameOverPanel, true);

            SetPlayerControl(false);
            UnlockCursor();
        }

        private void OnRestartClicked()
        {
            Debug.Log("Restart Button Clicked");

            // Reset Game State
            ResetGame();
        }
        #endregion

        #region Helpers
        private void SetPanelActive(GameObject panel, bool active)
        {
            if (panel != null)
            {
                panel.SetActive(active);
            }
        }

        private void SetPlayerControl(bool active)
        {
            if (_playerMovement != null)
            {
                _playerMovement.SetControl(active);
            }
        }

        private void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void ResetGame()
        {
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.buildIndex);
            Debug.Log("Game Reset Complete");
        }
        #endregion
    }
}