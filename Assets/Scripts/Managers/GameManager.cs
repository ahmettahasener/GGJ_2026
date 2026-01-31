using UnityEngine;
using System;

namespace GGJ_2026.Managers
{
    public enum GameState
    {
        MaskSelection,
        Gameplay,
        EndNight
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
                    HandleEndNight();
                    break;
            }
        }

        private void HandleMaskSelection()
        {
            // In a real scenario, this would enable the UI for selection.
            // For now, let's simulate selecting a random mask to proceed if we don't have the UI yet.
            Debug.Log("Mask Selection Phase Started.");
            
            // Example flow: 
            // 1. Get Options from MaskManager
            // 2. Show UI
            // 3. User clicks -> Calls ConfirmSelection(mask)
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
            // Enable controls, start timers
            Debug.Log("Night Started! Good luck.");
            
            // Refill resources if needed (Day Start Logic could be here or separate)
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.RefillEnergyForNight();
            }
        }

        public void EndNight()
        {
            ChangeState(GameState.EndNight);
        }

        private void HandleEndNight()
        {
            Debug.Log("Night Ended. Calculating stats...");

            if (ResourceManager.Instance != null && MaskManager.Instance != null)
            {
                // 1. Sanity Drop (Base 10 + Random)
                float sanityDrop = 10f + UnityEngine.Random.Range(0f, 10f);
                ResourceManager.Instance.ModifySanity(-sanityDrop);
                Debug.Log($"Sanity Dropped by {sanityDrop}");

                // 2. Monster Approach
                float monsterMove = 100f + UnityEngine.Random.Range(0f, 50f);
                
                // Risk Taker Effect
                if (MaskManager.Instance.IsEffectActive(Data.MaskType.RiskTaker))
                {
                    monsterMove *= 1.5f;
                    Debug.Log("RiskTaker active: Monster moves faster!");
                }
                
                // Observer Effect
                if (MaskManager.Instance.IsEffectActive(Data.MaskType.Observer))
                {
                    monsterMove *= 0.5f;
                    Debug.Log("Observer active: Monster feels watched, moves slower.");
                }

                ResourceManager.Instance.ModifyDistance(-monsterMove);
                Debug.Log($"Monster moved {monsterMove}m closer.");
            }

            // Prepare for next night
            Invoke(nameof(StartNewNight), 3f);
        }

        private void StartNewNight()
        {
            // Reset Mask? The design says "Mask effects apply for that night". 
            // Usually we clear it before selection.
            if (MaskManager.Instance != null)
                MaskManager.Instance.ClearMask();

            ChangeState(GameState.MaskSelection);
        }
    }
}
