using UnityEngine;
using System;
using GGJ_2026.UI;
using System.Collections;

namespace GGJ_2026.Managers
{
    public enum GameState
    {
        MaskSelection,
        Gameplay,
        EndNight,
        GameWin,
        GameOver
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public event Action<GameState> OnStateChanged;

        [SerializeField] private GameState _currentState;

        public GameState CurrentState => _currentState;

        [Header("Game Over Cinematic")]
        [SerializeField] private Transform _gameOverPlayerPoint;
        [SerializeField] private GameObject _monsterPrefab;
        [SerializeField] private Transform _monsterSpawnPoint;
        [SerializeField] private GameUI _gameUI;

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
            ChangeState(GameState.MaskSelection);
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                ChangeState(GameState.GameOver);
            }
        }

        public void ChangeState(GameState newState)
        {
            _currentState = newState;
            Debug.Log($"Game State Changed to: {_currentState}");
            OnStateChanged?.Invoke(_currentState);

            switch (_currentState)
            {
                case GameState.MaskSelection:
                    HandleMaskSelection();
                    break;
                case GameState.Gameplay:
                    HandleGameplay();
                    break;
                case GameState.EndNight:
                    StartCoroutine(EndNightSequence());
                    break;
                case GameState.GameWin:
                    Debug.Log("VICTORY! You contacted the outside world!");
                    break;
                case GameState.GameOver:
                    StartCoroutine(GameOverSequence());
                    Debug.Log("GAME OVER. The monster caught you.");
                    break;
            }
        }

        public int CurrentNight { get; private set; } = 1;

        private void HandleMaskSelection()
        {
            Debug.Log($"Mask Selection Phase Started. Night: {CurrentNight}");
            
            // Re-enable cursor for selection? 
            // MaskManager uses physical objects now, so we need Player Control!
            // But we need to UNLOCK cursor for interaction?
            // Actually, PlayerMovement locks cursor. 
            // Interaction system detects object -> E -> Animation.
            // So we just need to ensure Player is free to move.
            
            if (MaskManager.Instance != null)
            {
                MaskManager.Instance.StartSelectionPhase();
            }
        }

        // Called by MaskManager
        public void ConfirmMaskSelection(Data.MaskData selectedMask)
        {
            // Already handled in MaskManager selecting it then calling ChangeState
            ChangeState(GameState.Gameplay);
        }

        private void HandleGameplay()
        {
            Debug.Log($"Night {CurrentNight} Started! Good luck.");
            
            // Release Player if locked?
            // PlayerMovement handles its own state usually.
        }

        public void EndNight()
        {
            if (_currentState != GameState.EndNight)
            {
                ChangeState(GameState.EndNight);
            }
        }

        private System.Collections.IEnumerator EndNightSequence()
        {
            Debug.Log("Night Ending... Cinematic Sequence Start.");

            // 1. Lock everything (ESC block handled in PlayerInteract now)
            
            // 2. Fade In Black (Screen goes dark immediately)
            if (OverlayUI.Instance != null)
            {
                yield return StartCoroutine(OverlayUI.Instance.FadeIn(1.0f));
            }
            else
            {
                Debug.LogWarning("OverlayUI Instance not found!");
                yield return new WaitForSeconds(1.0f);
            }

            // 3. Show Night Text
            if (OverlayUI.Instance != null)
            {
                OverlayUI.Instance.SetNightText($"NIGHT {CurrentNight} SURVIVED");
            }

            // 4. Wait 3 seconds
            yield return new WaitForSeconds(3.0f);

            // 5. Calculations (Behind the curtain)
            CalculateNightResults();
            
            // Win/Loss Checks
            if (CheckGameOver())
            {
                ChangeState(GameState.GameOver);
                yield break; 
            }
            
            if (CheckGameWin())
            {
                ChangeState(GameState.GameWin);
                yield break; 
            }

            // 6. Transition Logic
            StartNewNight(); // Resets modifiers, increments night count
            
            if (OverlayUI.Instance != null)
            {
                OverlayUI.Instance.HideNightText();
            }

            // 7. Fade Out (Screen clears)
            if (OverlayUI.Instance != null)
            {
                yield return StartCoroutine(OverlayUI.Instance.FadeOut(1.0f));
            }

            // 8. Auto-Release Player from Bed (Force Exit Interaction)
            var playerInteract = FindObjectOfType<Interactions.PlayerInteract>();
            if (playerInteract != null)
            {
                playerInteract.ForceExitInteraction();
            }
        }

        private void CalculateNightResults()
        {
            if (ResourceManager.Instance != null)
            {
                // 1. Sanity Drop
                float sanityDrop = 10f + UnityEngine.Random.Range(0f, 10f);
                ResourceManager.Instance.ModifySanity(-sanityDrop);

                // 2. Monster Approach
                float baseMove = 100f + UnityEngine.Random.Range(0f, 50f);
                float finalMove = baseMove * ResourceManager.Instance.MonsterAdvanceMultiplier;
                ResourceManager.Instance.ModifyDistance(-finalMove);

                // 3. Refill Electricity
                ResourceManager.Instance.RefillEnergyForNight(); 
            }
        }

        private bool CheckGameOver()
        {
            if (ResourceManager.Instance != null)
            {
                if (ResourceManager.Instance.GetDistance() <= 0) return true;
                if (ResourceManager.Instance.GetSanity() <= 0) return false; // Optional
            }
            return false;
        }

        private bool CheckGameWin()
        {
            if (ResourceManager.Instance != null)
            {
                if (ResourceManager.Instance.GetFrequency() >= 100f) return true;
            }
            return false;
        }

        private void StartNewNight()
        {
            if (MaskManager.Instance != null)
                MaskManager.Instance.ClearMask();

            CurrentNight++;
            ChangeState(GameState.MaskSelection);
        }
        public void ResetGame()
        {
            CurrentNight = 1;
            ChangeState(GameState.MaskSelection);
        }
        private IEnumerator GameOverSequence()
        {
            Debug.Log("GAME OVER SEQUENCE STARTED");

            var playerInteract = FindObjectOfType<Interactions.PlayerInteract>();

            if (playerInteract != null && _gameOverPlayerPoint != null)
            {
                playerInteract.LockAndMovePlayerTo(_gameOverPlayerPoint);
            }

            // Sinematik nefes
            yield return new WaitForSeconds(0.5f);

            // Monster spawn
            if (_monsterPrefab != null && _monsterSpawnPoint != null)
            {
                GameObject monster = Instantiate(
                    _monsterPrefab,
                    _monsterSpawnPoint.position,
                    _monsterSpawnPoint.rotation
                );

                var controller = monster.GetComponent<MonsterGameOverController>();
                if (controller != null)
                {
                    controller.StartMovingToPlayer(
                        FindObjectOfType<GGJ_2026.Player.PlayerMovement>().transform,
                        OnMonsterReachedPlayer
                    );
                }
            }
        }

        private void OnMonsterReachedPlayer()
        {
            _gameUI.GameOver();
        }

    }
}
