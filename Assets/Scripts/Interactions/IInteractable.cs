using UnityEngine;

namespace GGJ_2026.Interactions
{
    public interface IInteractable
    {
        string InteractionPrompt { get; }
        Transform InteractionViewPoint { get; } // Target for camera
        bool UseCameraFocus { get; } // Should camera lock/focus?
        void OnInteract();
        void OnInteractStay(); // Called every frame while interacting
        void OnExit();
    }
}
