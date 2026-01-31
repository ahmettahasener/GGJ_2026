namespace GGJ_2026.Interactions
{
    public interface IInteractable
    {
        string InteractionPrompt { get; }
        void OnInteract();
        void OnExit();
    }
}
