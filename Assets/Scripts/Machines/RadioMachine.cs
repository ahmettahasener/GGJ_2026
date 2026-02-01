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

        [Header("Target Movement Settings")]
        [SerializeField] private float _targetBarMoveSpeed = 0.1f; // Ne kadar hızlı hareket edecek?
        [SerializeField] private float _targetMoveAmount = 1.3f; // Ne kadar geniş bir alanda salınacak?
        [SerializeField] private float _targetBarTargetPosition = 0; // Ne kadar geniş bir alanda salınacak?
        private float _initialTargetY; // Başlangıçtaki rastgele Y pozisyonu

        [Header("UI References")]
        [SerializeField] private GameObject _minigameCanvas; 
        [SerializeField] private RectTransform _backgroundRect;
        [SerializeField] private RectTransform _targetRect;
        [SerializeField] private RectTransform _playerRect;

        [Header("Audio")]
        [SerializeField] private AudioClip _startSound;
        [SerializeField] private AudioClip _successSound;

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
            //if (_minigameCanvas != null)
            //{
            //    _minigameCanvas.SetActive(false);
            //}
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


            //_targetRect.anchoredPosition = new Vector2(_targetRect.anchoredPosition.x, 0);
            _targetBarTargetPosition = SetRandomPosition();
            //// Reset Player Position to center
            //_playerRect.anchoredPosition = new Vector2(_playerRect.anchoredPosition.x, 0f);

            //Debug.Log($"Radio Mini-game Started. Target Y: {randomY} (Range: +/- {safeRangeY})");

            PlayMachineSound(_startSound);
        }

        // Called when 10s ends
        private void FinishSession()
        {
            if (_isSessionFinished) return;
            
            _isSessionFinished = true;
            // Record final state but DO NOT close canvas
            ApplyRewards();
            Debug.Log("Radio Session Finished. UI holding state.");
            
            StartCoroutine(WaitAndExit(2.0f));
        }

        private System.Collections.IEnumerator WaitAndExit(float delay)
        {
            yield return new WaitForSeconds(delay);
            ForceExit();
        }

        // Called when walking away
        private void CloseMiniGameFully()
        {
            _isMinigameActive = false;
            _isSessionFinished = false;
            //if (_minigameCanvas != null)
            //    _minigameCanvas.SetActive(false);
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

            // Block input if finished (extra safety though Update loop checks _isSessionFinished)
            if (_isSessionFinished) return;

            // --- 1. TARGET HAREKET MANTIĞI (DÜZELTİLMİŞ) ---
            Vector2 currentPos = _targetRect.anchoredPosition;

            // MoveTowards otomatik olarak hedefe doğru (hız * zaman) kadar ilerler.
            // Artı/eksi yönünü kendi ayarlar.
            float newY = Mathf.MoveTowards(currentPos.y, _targetBarTargetPosition, _targetBarMoveSpeed * dt);

            _targetRect.anchoredPosition = new Vector2(currentPos.x, newY);

            // Hedefe vardık mı? Çok yakınsak yeni hedef seç (Epsilon hatasını önlemek için 0.01f)
            if (Mathf.Abs(newY - _targetBarTargetPosition) < 0.01f)
            {
                _targetBarTargetPosition = SetRandomPosition();
                Debug.Log("Yeni Hedef Belirlendi: " + _targetBarTargetPosition);
            }

            //_targetRect.anchoredPosition = new Vector2(_targetRect.anchoredPosition.x, 1.3f);
            Debug.Log("anchored " + _targetRect.anchoredPosition.y);
            Debug.Log("local " + _targetRect.localPosition.y);

            // 1. Player Physics (Gravity vs Jump)
            if (Input.GetMouseButtonDown(0) || Input.GetKey(KeyCode.Space))
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

        private float SetRandomPosition()
        {
            float randomValue = Random.Range(-1.3f, 1.3f);
            Debug.Log("Random target bar value = " + randomValue);
            return randomValue;
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
            if (_successTimer > _sessionDuration * 0.5f) // %50+ başarı
            {
                PlayMachineSound(_successSound);
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
