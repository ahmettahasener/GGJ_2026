using UnityEngine;
using GGJ_2026.Managers;

namespace GGJ_2026.Machines
{
    public class RadioMachine : MachineBase
    {
        [Header("Radio Settings")]
        [SerializeField] private float _frequencyGainSpeed = 5f; // How fast frequency fills
        [SerializeField] private float _electricityConsumptionRate = 2f; // Per second

        [Header("Mini-Game Settings")]
        [SerializeField] private float _barGravity = 2f;
        [SerializeField] private float _barLiftForce = 5f;
        [SerializeField] private float _targetMoveSpeed = 1f;
        [SerializeField] private float _targetSize = 0.2f; // Percentage 0-1

        // Mini-game State
        private bool _isMinigameActive = false;
        private float _playerBarPos = 0.5f; // 0 to 1
        private float _targetBarPos = 0.5f; // 0 to 1
        private float _targetMoveTime = 0f;

        // UI References (Ideally these would be on a world-space canvas attached to the machine, 
        // but for now we might use Overlay or Debug logs, or assume a local reference)
        // For this task, I'll simulate the logic and log it, or assume there's a reference to a dedicated UI part.
        // The prompt asks for logic. I will add local visual feedback references.
        [Header("Visuals")]
        [SerializeField] private Transform _playerBarVisual; // Y-axis scaled or moved
        [SerializeField] private Transform _targetBarVisual; // Y-axis scaled or moved

        private void Update()
        {
            if (_isMinigameActive)
            {
                HandleMiniGame();
                ConsumeElectricityOverTime();
            }
        }

        public override void OnInteract()
        {
            base.OnInteract();
            if (!_isMinigameActive)
            {
                StartMiniGame();
            }
            else
            {
                StopMiniGame();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            StopMiniGame();
        }

        private void StartMiniGame()
        {
            _isMinigameActive = true;
            Debug.Log("Radio Mini-game Started! Keep the bar on the target!");
            
            // TODO: Lock Camera / Player Movement here. 
            // Since we don't have a direct Player reference yet, we'll assume the player respects the state 
            // or we lock the cursor.
            Cursor.lockState = CursorLockMode.None; // Unlock to show mouse interactions if needed or just use keys
            
            // Better: If using First Person, we usually want to FREEZE camera look.
            // Notify game/player manager if possible.
        }

        private void StopMiniGame()
        {
            if (!_isMinigameActive) return;

            _isMinigameActive = false;
            Debug.Log("Radio Mini-game Stopped.");
            Cursor.lockState = CursorLockMode.Locked; // Return to FPS lock
        }

        private void HandleMiniGame()
        {
            // 1. Target Movement (Simple Sine Wave + Noise for randomness)
            _targetMoveTime += Time.deltaTime * _targetMoveSpeed;
            _targetBarPos = Mathf.PerlinNoise(_targetMoveTime, 0f);
            
            // 2. Player Input (Space to lift)
            if (Input.GetKey(KeyCode.Space))
            {
                _playerBarPos += _barLiftForce * Time.deltaTime;
            }
            else
            {
                _playerBarPos -= _barGravity * Time.deltaTime;
            }

            _playerBarPos = Mathf.Clamp01(_playerBarPos);

            // 3. Check Overlap
            float diff = Mathf.Abs(_playerBarPos - _targetBarPos);
            bool isInside = diff < (_targetSize / 2f);

            if (isInside)
            {
                float gainMultiplier = 1f;

                if (MaskManager.Instance != null)
                {
                    // Mask: Risk Taker
                    if (MaskManager.Instance.IsEffectActive(Data.MaskType.RiskTaker))
                    {
                        gainMultiplier *= 2f;
                    }

                    // Mask: Madness Perk
                    if (MaskManager.Instance.IsEffectActive(Data.MaskType.MadnessPerk) && ResourceManager.Instance != null)
                    {
                        float sanity = ResourceManager.Instance.GetSanity();
                        // Example: Lower sanity = higher multiplier. 
                        // If Sanity 100 -> 1x. If Sanity 0 -> 2x.
                        float madnessBonus = Mathf.Lerp(2f, 1f, sanity / 100f);
                        gainMultiplier *= madnessBonus;
                    }
                }

                // Gain Frequency
                ResourceManager.Instance.ModifyFrequency(_frequencyGainSpeed * gainMultiplier * Time.deltaTime);
            }

            // Visual Updates (Pseudo-code for transform manipulation)
            if (_playerBarVisual != null)
                _playerBarVisual.localPosition = new Vector3(0, _playerBarPos, 0); // Example
            
            if (_targetBarVisual != null)
                _targetBarVisual.localPosition = new Vector3(0, _targetBarPos, 0); // Example
        }

        private void ConsumeElectricityOverTime()
        {
            if (ResourceManager.Instance != null && ResourceManager.Instance.GetElectricity() > 0)
            {
                ResourceManager.Instance.ModifyElectricity(-_electricityConsumptionRate * Time.deltaTime);
            }
            else
            {
                // Out of power, stop game
                StopMiniGame();
                Debug.Log("Radio stopped: Out of Electricity!");
            }
        }
    }
}
