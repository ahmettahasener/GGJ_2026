using UnityEngine;
using GGJ_2026.Interactions;
using GGJ_2026.Managers;
using GGJ_2026.UI; // Assuming access to managers might be needed later

namespace GGJ_2026.Machines
{
    public abstract class MachineBase : MonoBehaviour, IInteractable
    {
        [Header("Machine Settings")]
        [SerializeField] protected string _machineName = "Machine";
        [SerializeField] protected float _electricityCost = 10f;
        [SerializeField] protected bool _useCameraFocus = true;
        
        [Tooltip("The position/rotation where the camera should move to during interaction.")]
        [SerializeField] protected Transform _interactionViewPoint;

        [Header("Audio")]
        [SerializeField] protected AudioSource _audioSource;

        public string InteractionPrompt => $"Press \"E\" to use {_machineName}";
        public Transform InteractionViewPoint => _interactionViewPoint;
        public bool UseCameraFocus => _useCameraFocus;

        private void Awake()
        {
            // Auto-find AudioSource if not assigned
            if (_audioSource == null)
            {
                _audioSource = GetComponent<AudioSource>();
                if (_audioSource == null)
                {
                    _audioSource = gameObject.AddComponent<AudioSource>();
                }
            }
        }

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
            TryConsumeElectricity();
        }

        protected virtual bool TryConsumeElectricity()
        {
            if (ResourceManager.Instance != null)
            {
                if (!ResourceManager.Instance.IsPowerOn)
                {
                    Debug.Log("No Power! Machine won't work.");
                    if (OverlayUI.Instance != null) OverlayUI.Instance.ShowPrompt("No Power!");
                    return false;
                }

                // Check Cost
                if (ResourceManager.Instance.GetElectricity() >= _electricityCost)
                {
                    ResourceManager.Instance.ModifyElectricity(-_electricityCost);
                    Debug.Log($"{_machineName} consumed {_electricityCost} electricity.");
                    
                    if (GGJ_2026.UI.InfoPopupManager.Instance != null)
                    {
                        GGJ_2026.UI.InfoPopupManager.Instance.NotifyElectricity(-_electricityCost);
                    }

                    return true;
                }
                else
                {
                    Debug.Log("Not enough electricity!");
                    if (OverlayUI.Instance != null) OverlayUI.Instance.ShowPrompt("Not Enough Power!");
                    return false;
                }
            }
            return false;
        }
        protected void PlayMachineSound(AudioClip clip, float volume = 1f)
        {
            if (_audioSource != null && clip != null)
            {
                _audioSource.PlayOneShot(clip, volume);
            }
        }

        protected void PlayMachineSoundFromGroup(string groupName, float volume = 1f)
        {
            if (SoundManager.Instance != null && _audioSource != null)
            {
                SoundManager.Instance.PlayRandomSoundFromGroup(_audioSource, groupName, volume);
            }
        }
        protected void ForceExit()
        {
            // Find PlayerInteract - efficient enough for occasional calling
            PlayerInteract playerInteract = FindFirstObjectByType<PlayerInteract>();
            if (playerInteract != null)
            {
                playerInteract.ForceExitInteraction();
            }
        }
    }
}
