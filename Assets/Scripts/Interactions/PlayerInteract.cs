using UnityEngine;
using GGJ_2026.UI;

namespace GGJ_2026.Interactions
{
    public class PlayerInteract : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [SerializeField] private float _interactionDistance = 3f;
        [SerializeField] private LayerMask _interactionLayer;

        private Camera _cam;
        private IInteractable _currentInteractable;

        private GGJ_2026.Player.PlayerMovement _playerMovement;
        private bool _isInteracting = false;

        private void Awake()
        {
            _cam = Camera.main;
            if (_cam == null)
            {
                _cam = GetComponentInChildren<Camera>(); // Fallback
            }

            _playerMovement = GetComponent<GGJ_2026.Player.PlayerMovement>();
        }

        private void Update()
        {
            // If interacting, we wait for Exit command
            if (_isInteracting)
            {
                HandleExitInput();
                return;
            }

            HandleRaycast();
            HandleInput();
        }

        private void HandleExitInput()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ExitInteraction();
            }
        }

        private void ExitInteraction()
        {
            _isInteracting = false;
            
            if (_currentInteractable != null)
            {
                _currentInteractable.OnExit();
            }

            if (_playerMovement != null)
            {
                _playerMovement.SetControl(true);
            }
            
            // Re-enable UI prompt if still looking -> Raycast will handle next frame
            // But we might want to clear prompt momentarily
             UpdateUI(false);
        }

        private void HandleRaycast()
        {
            // Standard raycast code...
            Ray ray = new Ray(_cam.transform.position, _cam.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, _interactionDistance, _interactionLayer))
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();

                if (interactable != null)
                {
                    if (_currentInteractable != interactable)
                    {
                        _currentInteractable?.OnExit();
                        _currentInteractable = interactable;
                        UpdateUI(true, _currentInteractable.InteractionPrompt);
                    }
                }
                else
                {
                    ClearInteraction();
                }
            }
            else
            {
                ClearInteraction();
            }
        }

        private void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.E) && _currentInteractable != null)
            {
                StartInteraction();
            }
        }

        private void StartInteraction()
        {
            _isInteracting = true;
            _currentInteractable.OnInteract();
            
            // Hide Prompt
            UpdateUI(false);

            // Disable Movement
            if (_playerMovement != null)
            {
                _playerMovement.SetControl(false);
            }
        }

        private void ClearInteraction()
        {
            if (_currentInteractable != null)
            {
                _currentInteractable.OnExit();
                _currentInteractable = null;
                UpdateUI(false);
            }
        }

        private void UpdateUI(bool show, string msg = "")
        {
            if (OverlayUI.Instance != null)
            {
                if (show)
                    OverlayUI.Instance.ShowPrompt(msg);
                else
                    OverlayUI.Instance.HidePrompt();
            }
        }
    }
}
