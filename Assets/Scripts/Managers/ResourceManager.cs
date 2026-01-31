using UnityEngine;
using System;

namespace GGJ_2026.Managers
{
    public class ResourceManager : MonoBehaviour
    {
        public static ResourceManager Instance { get; private set; }

        // Resources
        [Header("Resources")]
        [SerializeField] private float _electricity = 100f;
        [SerializeField] private float _sanity = 100f;
        [SerializeField] private float _distance = 1000f;
        [SerializeField] private float _frequency = 0f;

        // Max/Min Values
        private const float MaxElectricity = 100f;
        private const float MaxSanity = 100f;
        private const float StartDistance = 1000f;
        private const float MaxFrequency = 100f;

        // Events
        public event Action<float> OnElectricityChanged;
        public event Action<float> OnSanityChanged;
        public event Action<float> OnDistanceChanged;
        public event Action<float> OnFrequencyChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                // Note: Not using DontDestroyOnLoad here if it shares the object with GameManager or if architecture dictates per-scene logic, 
                // but usually Managers persist. Assuming per-scene reset or persistent based on context. 
                // Creating it as persistent for safety alongside GameManager.
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Initialize UI
            UpdateAllUI();
        }

        public void ModifyElectricity(float amount)
        {
            _electricity = Mathf.Clamp(_electricity + amount, 0, MaxElectricity);
            OnElectricityChanged?.Invoke(_electricity);
        }

        public void ModifySanity(float amount)
        {
            _sanity = Mathf.Clamp(_sanity + amount, 0, MaxSanity);
            OnSanityChanged?.Invoke(_sanity);
        }

        public void ModifyDistance(float amount)
        {
            _distance = Mathf.Max(0, _distance + amount); // No upper limit defined, but can't be negative
            OnDistanceChanged?.Invoke(_distance);

            if (_distance <= 0)
            {
                Debug.Log("Game Over: Monster reached the tower!");
                // Trigger Game Over Logic via GameManager
            }
        }

        public void ModifyFrequency(float amount)
        {
            _frequency = Mathf.Clamp(_frequency + amount, 0, MaxFrequency);
            OnFrequencyChanged?.Invoke(_frequency);

            if (_frequency >= MaxFrequency)
            {
                Debug.Log("Victory: Help contacted!");
                // Trigger Win Logic via GameManager
            }
        }

        public float GetElectricity() => _electricity;
        public float GetSanity() => _sanity;
        public float GetDistance() => _distance;
        public float GetFrequency() => _frequency;

        // Power System
        public bool IsPowerOn { get; private set; } = true;
        public float NextNightMaxElectricity { get; private set; } = 100f;

        public event Action<bool> OnPowerStateChanged;

        private void UpdateAllUI()
        {
            OnElectricityChanged?.Invoke(_electricity);
            OnSanityChanged?.Invoke(_sanity);
            OnDistanceChanged?.Invoke(_distance);
            OnFrequencyChanged?.Invoke(_frequency);
            OnPowerStateChanged?.Invoke(IsPowerOn);
        }

        // Modifiers (Reset every night)
        public float MonsterAdvanceMultiplier { get; private set; } = 1f;
        public float RadioFrequencyMultiplier { get; private set; } = 1f;
        public float RadarPushMultiplier { get; private set; } = 1f;
        public float RadarCostMultiplier { get; private set; } = 1f;
        public float RadarFuseChanceMultiplier { get; private set; } = 1f;
        public float MedicineCostMultiplier { get; private set; } = 1f;
        public bool IsFuseBlockActive { get; private set; } = false;

        public void ResetModifiers()
        {
            MonsterAdvanceMultiplier = 1f;
            RadioFrequencyMultiplier = 1f;
            RadarPushMultiplier = 1f;
            RadarCostMultiplier = 1f;
            RadarFuseChanceMultiplier = 1f;
            MedicineCostMultiplier = 1f;
            IsFuseBlockActive = false;
            
            Debug.Log("ResourceManager: Modifiers Reset.");
        }

        // --- Setter Methods for MaskManager ---
        public void SetMonsterAdvanceMultiplier(float val) => MonsterAdvanceMultiplier = val;
        public void SetRadioFrequencyMultiplier(float val) => RadioFrequencyMultiplier = val;
        public void SetRadarPushMultiplier(float val) => RadarPushMultiplier = val;
        public void SetRadarCostMultiplier(float val) => RadarCostMultiplier = val;
        public void SetRadarFuseChanceMultiplier(float val) => RadarFuseChanceMultiplier = val;
        public void SetMedicineCostMultiplier(float val) => MedicineCostMultiplier = val;
        public void SetFuseBlockActive(bool val) => IsFuseBlockActive = val;

        public void TriggerPowerOutage()
        {
            if (!IsPowerOn) return;

            // Check Modifier: Fuse Block (Durable Line)
            if (IsFuseBlockActive)
            {
                Debug.Log("Fuse would have blown, but Block is Active (Durable Line)!");
                return;
            }

            IsPowerOn = false;
            OnPowerStateChanged?.Invoke(IsPowerOn);
            Debug.Log("POWER OUTAGE! Fuse blown.");
        }

        public void RestorePower(float initialAmount, float nextNightCap)
        {
            IsPowerOn = true;
            _electricity = initialAmount;
            NextNightMaxElectricity = nextNightCap;

            OnPowerStateChanged?.Invoke(IsPowerOn);
            OnElectricityChanged?.Invoke(_electricity);
            Debug.Log($"Power Restored. Next night capacity reduced to {NextNightMaxElectricity}%.");
        }

        public void RefillEnergyForNight()
        {
            IsPowerOn = true;
            _electricity = NextNightMaxElectricity;
            // Reset cap for following night standard (unless changed again) 
            NextNightMaxElectricity = 100f;

            UpdateAllUI();
        }
    }
}
