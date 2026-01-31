using UnityEngine;
using GGJ_2026.Managers;

namespace GGJ_2026.Machines
{
    public class RadarMachine : MachineBase
    {
        [Header("Radar Settings")]
        [SerializeField] private float _baseDistancePushAmount = 50f;
        [SerializeField] private float _fuseBlowChance = 0.20f; // 20%
        [SerializeField] private float _electricityConsumptionRate = 2f; // Per second

        [Header("Mini-Game Configuration")]
        [SerializeField] private float _cursorSpeed = 200f; // Cursor movement speed
        [SerializeField] private float _maxCursorDistance = 150f; // Max distance from center
        [SerializeField] private float _perfectHitRadius = 20f; // Radius for max reward

        [Header("UI References")]
        [SerializeField] private GameObject _minigameCanvas;
        [SerializeField] private RectTransform _dartBackground; // Dart board sprite
        [SerializeField] private RectTransform _cursorRect; // Moving cursor sprite
        [SerializeField] private RectTransform _centerPoint; // Center reference (can be same as dartBackground)

        // Mini-game State
        private bool _isMinigameActive = false;
        private bool _isSessionFinished = false;
        private Vector2 _cursorVelocity;
        private Vector2 _targetDirection;
        private float _directionChangeTimer = 0f;
        private float _directionChangeInterval = 0.5f; // Change direction every 0.5s

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

            // Check Power First
            if (ResourceManager.Instance != null && !ResourceManager.Instance.IsPowerOn)
            {
                Debug.Log("Radar clicked: Power is OUT!");
                return;
            }

            if (!_isMinigameActive)
            {
                StartMiniGame();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            CloseMiniGameFully();
        }

        private void StartMiniGame()
        {
            if (_minigameCanvas == null || _dartBackground == null || _cursorRect == null)
            {
                Debug.LogError("RadarMachine: UI References missing!");
                return;
            }

            _isMinigameActive = true;
            _isSessionFinished = false;
            _minigameCanvas.SetActive(true);

            // Reset cursor to center
            _cursorRect.anchoredPosition = Vector2.zero;

            // Initialize random movement
            _directionChangeTimer = 0f;
            ChangeRandomDirection();

            Debug.Log("Radar Mini-game Started. Click to stop the cursor!");
        }

        private void FinishSession(float accuracy)
        {
            if (_isSessionFinished) return;

            _isSessionFinished = true;
            ApplyRadarEffect(accuracy);

            // Close canvas after a brief delay to show final position
            Invoke(nameof(CloseMiniGameFully), 0.5f);
        }

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

            // Check for user click to stop
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                float accuracy = CalculateAccuracy();
                FinishSession(accuracy);
                return;
            }

            // Random direction change timer
            _directionChangeTimer += dt;
            if (_directionChangeTimer >= _directionChangeInterval)
            {
                ChangeRandomDirection();
                _directionChangeTimer = 0f;
            }

            // Move cursor
            Vector2 newPos = _cursorRect.anchoredPosition;
            newPos += _targetDirection * _cursorSpeed * dt;

            // Clamp cursor within max distance from center
            if (newPos.magnitude > _maxCursorDistance)
            {
                newPos = newPos.normalized * _maxCursorDistance;
                ChangeRandomDirection(); // Bounce back with new direction
            }

            _cursorRect.anchoredPosition = newPos;
        }

        private void ChangeRandomDirection()
        {
            // Generate random direction
            float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            _targetDirection = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle)).normalized;
        }

        private float CalculateAccuracy()
        {
            // Calculate distance from center
            Vector2 centerPos = _centerPoint != null ? _centerPoint.anchoredPosition : Vector2.zero;
            float distanceFromCenter = Vector2.Distance(_cursorRect.anchoredPosition, centerPos);

            // Calculate accuracy (1.0 at center, 0.0 at max distance)
            float accuracy = 1f - Mathf.Clamp01(distanceFromCenter / _maxCursorDistance);

            // Bonus for perfect hit
            if (distanceFromCenter <= _perfectHitRadius)
            {
                accuracy = 1f;
            }

            Debug.Log($"Cursor stopped at distance: {distanceFromCenter:F1}, Accuracy: {accuracy:P0}");
            return accuracy;
        }

        private void ApplyRadarEffect(float accuracy)
        {
            if (ResourceManager.Instance == null) return;

            // Calculate push amount: base amount + bonus based on accuracy
            // Base amount is guaranteed, bonus is added on top
            float bonusAmount = _baseDistancePushAmount * accuracy;
            float pushAmount = _baseDistancePushAmount + bonusAmount;
            float currentFuseChance = _fuseBlowChance;

            // Apply multipliers
            pushAmount *= ResourceManager.Instance.RadarPushMultiplier;
            currentFuseChance *= ResourceManager.Instance.RadarFuseChanceMultiplier;

            // 1. Push Distance (always at least base amount)
            ResourceManager.Instance.ModifyDistance(pushAmount);
            Debug.Log($"Radar Used: Monster pushed back {pushAmount:F1}m (Base: {_baseDistancePushAmount}, Bonus: {bonusAmount:F1}, Accuracy: {accuracy:P0})");

            // 2. Chance to Blow Fuse (regardless of accuracy)
            if (currentFuseChance > 0f)
            {
                float random = Random.value;
                if (random < currentFuseChance)
                {
                    ResourceManager.Instance.TriggerPowerOutage();
                    Debug.Log("Radar blew the fuse!");
                }
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
                    Debug.Log("Radar: Power cut during mini-game!");
                }
            }
        }

        // Override UseMachine to prevent base behavior
        protected override void UseMachine()
        {
            // Mini-game handles everything, so we don't need base UseMachine logic
            // Base OnInteract is enough for interaction detection
        }
    }
}