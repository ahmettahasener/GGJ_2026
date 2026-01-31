using UnityEngine;
using GGJ_2026.Interactions;
using GGJ_2026.Managers; // Assuming access to managers might be needed later

namespace GGJ_2026.Machines
{
    public abstract class MachineBase : MonoBehaviour, IInteractable
    {
        [Header("Machine Settings")]
        [SerializeField] protected string _machineName = "Machine";
        [SerializeField] protected float _electricityCost = 10f;
        
        [Tooltip("The position/rotation where the camera should move to during interaction.")]
        [SerializeField] protected Transform _interactionViewPoint;

        public string InteractionPrompt => $"Press \"E\" to use {_machineName}";
        public Transform InteractionViewPoint => _interactionViewPoint;

        public virtual void OnInteract()
        {
            Debug.Log($"Interacted with {_machineName}");
            UseMachine();
        }

        public virtual void OnInteractStay()
        {
            // Optional override for mini-games
        }

        public virtual void OnExit()
        {
            // Optional visual reset or UI hide logic if driven by machine
            Debug.Log($"Walked away from {_machineName}");
        }

        protected virtual void UseMachine()
        {
            // Core logic for machine usage
            ConsumeElectricity();
        }

        protected virtual void ConsumeElectricity()
        {
            if (ResourceManager.Instance != null)
            {
                if (!ResourceManager.Instance.IsPowerOn)
                {
                    Debug.Log("No Power! Machine won't work.");
                    return;
                }

                // Simple check before consumption
                if (ResourceManager.Instance.GetElectricity() >= _electricityCost)
                {
                    ResourceManager.Instance.ModifyElectricity(-_electricityCost);
                    Debug.Log($"{_machineName} consumed {_electricityCost} electricity.");
                }
                else
                {
                    Debug.Log("Not enough electricity!");
                }
            }
        }
    }
}
