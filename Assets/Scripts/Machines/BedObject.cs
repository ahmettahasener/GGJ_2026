using UnityEngine;
using GGJ_2026.Interactions;
using GGJ_2026.Managers;

namespace GGJ_2026.Machines
{
    public class BedObject : MonoBehaviour, IInteractable
    {
        [SerializeField] private Transform _viewPoint;
        
        public string InteractionPrompt => "Press \"E\" to Sleep";
        public Transform InteractionViewPoint => _viewPoint;

        public void OnInteract()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Gameplay)
            {
                Debug.Log("Player interacting with Bed. Ending Night.");
                GameManager.Instance.EndNight();
            }
        }

        public void OnExit()
        {
            // Optional: Any logic needed when cancelling sleep view?
        }
    }
}
