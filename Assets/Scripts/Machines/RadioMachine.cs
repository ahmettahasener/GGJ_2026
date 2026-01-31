using UnityEngine;
using GGJ_2026.Managers;

namespace GGJ_2026.Machines
{
    public class RadioMachine : MachineBase
    {
        [Header("Radio Settings")]
        [SerializeField] private float _maxFrequencyReward = 20f; // Max frequency gained for 100% success
        [SerializeField] private float _electricityConsumptionRate = 2f; // Per second

        [Header("Mini-Game Configuration")]
        [SerializeField] private float _sessionDuration = 10f;
        [SerializeField] private float _gravity = 800f; 
        [SerializeField] private float _liftForce = 1200f;
        [SerializeField] private float _targetPadding = 10f; // Extra padding inside background

        [Header("UI References")]
        [SerializeField] private GameObject _minigameCanvas; 
        [SerializeField] private RectTransform _backgroundRect;
        [SerializeField] private RectTransform _targetRect;
        [SerializeField] private RectTransform _playerRect;

        // Mini-game State
        private bool _isMinigameActive = false;
        private bool _isSessionFinished = false; // New state to freeze UI but keep it open
        private float _playerVelocityY = 0f;
        private float _sessionTimer = 0f;
        private float _successTimer = 0f;

        // Cache height for performance
        private float _bgHeight;
        private float _targetHeight;
        private float _playerHeight;

        private void Start()
        {
            if (_minigameCanvas != null)
            {
                _minigameCanvas.SetActive(false);
            }
        }

        private void Update()
        {
            if (_isMinigameActive && !_isSessionFinished)
            {
                HandleMiniGame();
                ConsumeElectricityOverTime();
            }
        }

        public override void OnInteract()
        {
            base.OnInteract();
            // Allow restart if finished, or toggle? 
            // User flow: Interact -> Play 10s -> Freeze -> Interact to Close? Or Interact to Restart?
            // "Makinadan çıkınca tekrardan o canvası kapatsın" -> implies OnExit closes.
            // If we interact WHILE it's finished, maybe we restart?
            if (!_isMinigameActive)
            {
                StartMiniGame();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            // Always close everything on exit
            CloseMiniGameFully();
        }

        private void StartMiniGame()
        {
            if (_minigameCanvas == null || _backgroundRect == null || _targetRect == null || _playerRect == null)
            {
                Debug.LogError("RadioMachine: UI References missing!");
                return;
            }

            _isMinigameActive = true;
            _isSessionFinished = false;
            _minigameCanvas.SetActive(true);

            // Reset Timers
            _sessionTimer = 0f;
            _successTimer = 0f;
            _playerVelocityY = 0f;

            // Cache dimensions
            _bgHeight = _backgroundRect.rect.height;
            _targetHeight = _targetRect.rect.height;
            _playerHeight = _playerRect.rect.height;

            // 1. Randomize Target Position (Strict Bounds)
            // Available vertical space for the CENTER of the target
            // Total Height - Target Height gives the range of motion for the edges.
            // Halve it for center offset from (0,0).
            float safeRangeY = (_bgHeight - _targetHeight) * 0.5f;
            
            // Subtract padding to be extra safe
            safeRangeY -= _targetPadding;

            if (safeRangeY < 0) safeRangeY = 0; // Prevent negative range if target > bg

            // Randomize between -Range and +Range (Assuming Center Pivot)
            float randomY = Random.Range(-safeRangeY, safeRangeY);
            
            _targetRect.anchoredPosition = new Vector2(_targetRect.anchoredPosition.x, randomY);

            // Reset Player Position to center
            _playerRect.anchoredPosition = new Vector2(_playerRect.anchoredPosition.x, 0f);

            Debug.Log($"Radio Mini-game Started. Target Y: {randomY} (Range: +/- {safeRangeY})");
        }

        // Called when 10s ends
        private void FinishSession()
        {
            if (_isSessionFinished) return;
            
            _isSessionFinished = true;
            // Record final state but DO NOT close canvas
            ApplyRewards();
            Debug.Log("Radio Session Finished. UI holding state.");
        }

        // Called when walking away
        private void CloseMiniGameFully()
        {
            _isMinigameActive = false;
            _isSessionFinished = false;
            if (_minigameCanvas != null)
                _minigameCanvas.SetActive(false);
        }

        private void HandleMiniGame()
        {
            float dt = Time.deltaTime;
            _sessionTimer += dt;

            // Check for End of Session
            if (_sessionTimer >= _sessionDuration)
            {
                FinishSession();
                return;
            }

            // 1. Player Physics (Gravity vs Jump)
            if (Input.GetKey(KeyCode.Space))
            {
                _playerVelocityY += _liftForce * dt;
            }
            else
            {
                _playerVelocityY -= _gravity * dt;
            }

            // Apply Velocity
            Vector2 newPos = _playerRect.anchoredPosition;
            newPos.y += _playerVelocityY * dt;

            // 2. Clamp Player to Background
            // Assuming anchors are centered
            float maxY = (_bgHeight * 0.5f) - (_playerHeight * 0.5f);
            float minY = -(_bgHeight * 0.5f) + (_playerHeight * 0.5f);
            
            if (newPos.y > maxY)
            {
                newPos.y = maxY;
                _playerVelocityY = 0f; 
            }
            else if (newPos.y < minY)
            {
                newPos.y = minY;
                _playerVelocityY = 0f; 
            }

            _playerRect.anchoredPosition = newPos;

            // 3. Check Overlap (Success Logic)
            if (CheckOverlap())
            {
                _successTimer += dt;
                // Optional: You could change color here if overlapped
            }
        }

        private bool CheckOverlap()
        {
            float playerY = _playerRect.anchoredPosition.y;
            float targetY = _targetRect.anchoredPosition.y; 
            
            // Check if Player is INSIDE Target
            // Target is the container, Player is the content to hold inside.
            // Target Top > Player Top AND Target Bottom < Player Bottom
            
            float targetTop = targetY + (_targetHeight * 0.5f);
            float targetBottom = targetY - (_targetHeight * 0.5f);
            float playerTop = playerY + (_playerHeight * 0.5f);
            float playerBottom = playerY - (_playerHeight * 0.5f);

            return (playerTop <= targetTop && playerBottom >= targetBottom);
        }

        private void ApplyRewards()
        {
            float successRatio = _successTimer / _sessionDuration;
            float reward = _maxFrequencyReward * successRatio;

            float finalMultiplier = 1f;

            if (ResourceManager.Instance != null)
            {
                // Base Multiplier (includes Risk Taker)
                finalMultiplier *= ResourceManager.Instance.RadioFrequencyMultiplier;

                // Dynamic Check: Madness Perk
                // We still check MaskManager for the type because this is a dynamic stat-based effect
                if (MaskManager.Instance != null && MaskManager.Instance.IsEffectActive(Data.MaskType.MadnessPerk))
                {
                    float sanity = ResourceManager.Instance.GetSanity();
                    // If Sanity 100 -> 1x. If Sanity 0 -> 2x.
                    float madnessBonus = Mathf.Lerp(2f, 1f, sanity / 100f);
                    finalMultiplier *= madnessBonus;
                }
            }

            reward *= finalMultiplier;

            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.ModifyFrequency(reward);
                Debug.Log($"Radio Reward: {reward:F1}% Freq (Success: {successRatio:P0})");
            }
        }

        private void ConsumeElectricityOverTime()
        {
            if (ResourceManager.Instance != null)
            {
                if (ResourceManager.Instance.GetElectricity() > 0)
                {
                    ResourceManager.Instance.ModifyElectricity(-_electricityConsumptionRate * Time.deltaTime);
                }
                else
                {
                    CloseMiniGameFully(); // Power cut aborts completely
                }
            }
        }
    }
}
