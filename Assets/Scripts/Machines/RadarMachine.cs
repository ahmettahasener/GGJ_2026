using UnityEngine;
using GGJ_2026.Managers;

namespace GGJ_2026.Machines
{
    public class RadarMachine : MachineBase
    {
        [Header("Radar Settings")]
        [SerializeField] private float _distancePushAmount = 50f;
        [SerializeField] private float _fuseBlowChance = 0.20f; // 20%

        public override void OnInteract()
        {
            // Check Power First
            if (ResourceManager.Instance != null && !ResourceManager.Instance.IsPowerOn)
            {
                Debug.Log("Radar clicked: Power is OUT!");
                return;
            }

            // Regular Interaction check logic from base is:
            // base.OnInteract calls UseMachine calls ConsumeElectricity which does the check but after we might want distinct logic.
            // But here we want ONCE execution (Push).

            // Let's modify logic: Base.OnInteract will call UseMachine().
            base.OnInteract(); 
        }

        protected override void UseMachine()
        {
            if (ResourceManager.Instance == null) return;
            if (!ResourceManager.Instance.IsPowerOn) return;

            float actualCost = _electricityCost;

            // Mask: Efficient Radar
            if (MaskManager.Instance != null && MaskManager.Instance.IsEffectActive(Data.MaskType.EfficientRadar))
            {
                actualCost *= 0.2f; // Very low cost
            }

            if (ResourceManager.Instance.GetElectricity() >= actualCost)
            {
                ResourceManager.Instance.ModifyElectricity(-actualCost);
                ApplyRadarEffect();
            }
            else
            {
                Debug.Log("Radar Failed: Not enough power.");
            }
        }

        private void ApplyRadarEffect()
        {
            float pushAmount = _distancePushAmount;
            float currentFuseChance = _fuseBlowChance;

            if (MaskManager.Instance != null)
            {
                // Mask: Strong Pusher
                if (MaskManager.Instance.IsEffectActive(Data.MaskType.StrongPusher))
                {
                    pushAmount *= 1.5f;
                }

                // Mask: Efficient Radar (No fuse blow)
                if (MaskManager.Instance.IsEffectActive(Data.MaskType.EfficientRadar))
                {
                    currentFuseChance = 0f;
                }
            }

            // 1. Push Distance
            ResourceManager.Instance.ModifyDistance(pushAmount);
            Debug.Log($"Radar Used: Monster pushed back {pushAmount}m.");

            // 2. Chance to Blow Fuse
            if (currentFuseChance > 0f)
            {
                float random = Random.value;
                if (random < currentFuseChance)
                {
                    ResourceManager.Instance.TriggerPowerOutage();
                }
            }
        }
        
        // Correcting the override logic:
        // I should NOT call base.UseMachine() if I want custom logic that depends on consumption success.
        // OR I modify MachineBase to return bool inside ConsumeElectricity.
        // Given constraints, I will override UseMachine and NOT call base.UseMachine(), but handle consumption myself.
        
        /*
        protected override void UseMachine() {
            if (ResourceManager.Instance.IsPowerOn && ResourceManager.Instance.GetElectricity() >= _electricityCost) {
                ResourceManager.Instance.ModifyElectricity(-_electricityCost);
                ApplyRadarEffect();
            }
        }
        */
    }
}
