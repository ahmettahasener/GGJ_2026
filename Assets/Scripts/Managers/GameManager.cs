using UnityEngine;
using System;

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
                    Debug.Log("GAME OVER. The monster caught you.");
                    break;
            }
        }

        private void HandleMaskSelection()
        {
            Debug.Log("Mask Selection Phase Started.");
            
            if (MaskManager.Instance != null)
            {
                MaskManager.Instance.StartSelectionPhase();
            }
        }

        // Called by UI
        public void ConfirmMaskSelection(Data.MaskData selectedMask)
        {
            if (MaskManager.Instance != null)
                MaskManager.Instance.ActivateMask(selectedMask);
            
            ChangeState(GameState.Gameplay);
        }

        private void HandleGameplay()
        {
            Debug.Log("Night Started! Good luck.");
            
            // Note: Electricity Refill happens at the END of the previous night (or start of day logic)
            // But for the very first night, we assume values are set in Inspector.
        }

        public void EndNight()
        {
            ChangeState(GameState.EndNight);
        }

        private System.Collections.IEnumerator EndNightSequence()
        {
            Debug.Log("Night Ending... Fading out (imagined).");
            
            // Simulate Fade Out time
            yield return new WaitForSeconds(2.0f);

            CalculateNightResults();
            
            // Check Win / Loss
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

            yield return new WaitForSeconds(1.0f); // Breathing room
            StartNewNight();
        }

        private void CalculateNightResults()
        {
            if (ResourceManager.Instance != null)
            {
                // 1. Sanity Drop (Base 10 + Random 0-10)
                float sanityDrop = 10f + UnityEngine.Random.Range(0f, 10f);
                ResourceManager.Instance.ModifySanity(-sanityDrop);
                Debug.Log($"Sanity Dropped by {sanityDrop:F1}");

                // 2. Monster Approach (Base 100 + Random 0-50) * Multiplier
                float baseMove = 100f + UnityEngine.Random.Range(0f, 50f);
                float finalMove = baseMove * ResourceManager.Instance.MonsterAdvanceMultiplier;
                
                ResourceManager.Instance.ModifyDistance(-finalMove);
                Debug.Log($"Monster moved {finalMove:F1}m closer (Base: {baseMove:F1}, Mult: {ResourceManager.Instance.MonsterAdvanceMultiplier}).");

                // 3. Refill Electricity for Next Night
                // If power is out, we might want to punish? But prompt says standard logic:
                // "Restore to NextNightMaxElectricity (set by fuse repair or default)"
                ResourceManager.Instance.RefillEnergyForNight(); 
                Debug.Log("Electricity Refilled for next night.");
            }
        }

        private bool CheckGameOver()
        {
            if (ResourceManager.Instance != null)
            {
                // Condition: Distance <= 0
                if (ResourceManager.Instance.GetDistance() <= 0) return true;
                
                // Optional: Sanity <= 0?
                if (ResourceManager.Instance.GetSanity() <= 0) 
                {
                    Debug.Log("Sanity reached 0! (Game Over condition potentially)");
                    // return true; // Unleash if desired
                }
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

            ChangeState(GameState.MaskSelection);
        }
    }
}
