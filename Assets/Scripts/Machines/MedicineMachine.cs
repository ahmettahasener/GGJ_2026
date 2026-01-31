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

        private bool _isProcessing = false;

        protected override void UseMachine()
        {
            if (_isProcessing) return;
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

            // 2. Check Cost
            if (ResourceManager.Instance.GetElectricity() >= actualCost)
            {
                // Start Processing
                StartCoroutine(ProcessMedicine(actualCost));
            }
            else
            {
                Debug.Log("Medicine Machine: Not enough electricity.");
            }
        }

        private System.Collections.IEnumerator ProcessMedicine(float cost)
        {
            _isProcessing = true;
            Debug.Log("Dispensing Medicine... (3s)");
            
            // Consume immediately or after? Usually interact = consume.
            if (ResourceManager.Instance != null)
                ResourceManager.Instance.ModifyElectricity(-cost);

            // Wait 3 seconds
            yield return new WaitForSeconds(3.0f);

            // Apply Effect
            ApplyMedicineEffect();

            _isProcessing = false;

            // Auto Exit
            ForceExit();
        }

        private void ApplyMedicineEffect()
        {
            // Restore Sanity
            ResourceManager.Instance.ModifySanity(_sanityRestoreAmount);
            Debug.Log($"Medicine Used: Sanity increased by {_sanityRestoreAmount}");

            // Audio
            //if (SoundManager.Instance != null && _dispenseSound != null)
            //{
            //    SoundManager.Instance.PlaySound(_dispenseSound);
            //}

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
