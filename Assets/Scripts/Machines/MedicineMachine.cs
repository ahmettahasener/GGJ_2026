using UnityEngine;
using GGJ_2026.Managers;

namespace GGJ_2026.Machines
{
    public class MedicineMachine : MachineBase
    {
        [Header("Medicine Settings")]
        [SerializeField] private float _sanityRestoreAmount = 20f;
        [SerializeField] private AudioClip _dispenseSound;

        [Header("Visual Effects")]
        // Simple reference to a flash overlay or similar. 
        // For now, could be a localized particle or just a debug/event.
        [SerializeField] private ParticleSystem _visualEffect;

        protected override void UseMachine()
        {
            if (ResourceManager.Instance == null) return;

            // 1. Check Power
            if (!ResourceManager.Instance.IsPowerOn)
            {
                Debug.Log("Medicine Machine: No Power!");
                return;
            }

            float actualCost = _electricityCost;
            
            if (ResourceManager.Instance != null)
            {
                actualCost *= ResourceManager.Instance.MedicineCostMultiplier;
            }

            // 2. Check Cost & Consume
            if (ResourceManager.Instance.GetElectricity() >= actualCost)
            {
                ResourceManager.Instance.ModifyElectricity(-actualCost);
                
                // 3. Apply Effect
                ApplyMedicineEffect();
            }
            else
            {
                Debug.Log("Medicine Machine: Not enough electricity.");
            }
        }

        private void ApplyMedicineEffect()
        {
            // Restore Sanity
            ResourceManager.Instance.ModifySanity(_sanityRestoreAmount);
            Debug.Log($"Medicine Used: Sanity increased by {_sanityRestoreAmount}");

            // Audio
            if (SoundManager.Instance != null && _dispenseSound != null)
            {
                SoundManager.Instance.PlaySound(_dispenseSound);
            }

            // Visual
            if (_visualEffect != null)
            {
                _visualEffect.Play();
            }
            // If we had a PostProcessingManager or UIManager for flashes, we'd call it here.
            // Example: UIManager.Instance.FlashScreen(Color.green);
        }
    }
}
