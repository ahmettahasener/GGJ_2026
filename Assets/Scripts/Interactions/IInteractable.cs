using UnityEngine;

namespace GGJ_2026.Interactions
{
    public interface IInteractable
    {
        string InteractionPrompt { get; }
        Transform InteractionViewPoint { get; } // Target for camera
        void OnInteract();
        void OnExit();
    }
}
