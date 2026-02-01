using UnityEngine;
using GGJ_2026.UI;
using System.Collections;
using GGJ_2026.Managers;

namespace GGJ_2026.Interactions
{
    public class PlayerInteract : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [SerializeField] private float _interactionDistance = 3f;
        [SerializeField] private LayerMask _interactionLayer;

        [Header("Camera Transition")]
        [SerializeField] private float _transitionDuration = 1.0f;
        [SerializeField] private AnimationCurve _transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private Camera _cam;
        private IInteractable _currentInteractable;

        private GGJ_2026.Player.PlayerMovement _playerMovement;
        private bool _isInteracting = false;
        private bool _isFocused = false; // Tracks if we are in a camera-locked state

        // Camera State
        private Vector3 _originalLocalPos;
        private Quaternion _originalLocalRot;
        private Coroutine _cameraCoroutine;

        private MaskManager maskManager;

        private void Awake()
        {
            _cam = Camera.main;
            if (_cam == null)
            {
                _cam = GetComponentInChildren<Camera>(); // Fallback
            }

            _playerMovement = GetComponent<GGJ_2026.Player.PlayerMovement>();

            // Find MaskManager safely
            maskManager = FindObjectOfType<MaskManager>();
        }

        private void OnEnable()
        {
            if (maskManager != null)
                maskManager.cardSelectedEvent += ExitInteraction;
        }

        private void OnDisable()
        {
            if (maskManager != null)
                maskManager.cardSelectedEvent -= ExitInteraction;
        }

        private void Update()
        {
            // If interacting, we wait for Exit command AND allow internal logic
            if (_isInteracting)
            {
                HandleExitInput();
                if (_currentInteractable != null)
                {
                    // Enforce cursor for minigames that need it
                    // Only enforce if we are FOCUSED (i.e., minigame mode)
                    if (_isFocused) 
                    {
                        Cursor.lockState = CursorLockMode.None;
                        Cursor.visible = true;
                    }
                    
                    _currentInteractable.OnInteractStay();
                }
                return;
            }

            HandleRaycast();
            HandleInput();
        }

        public void ForceExitInteraction()
        {
            ExitInteraction();
        }

        private void HandleExitInput()
        {
            // Block ESC if we are in EndNight state (handled by GameManager logic or check here)
            if (Managers.GameManager.Instance != null && Managers.GameManager.Instance.CurrentState == Managers.GameState.EndNight)
            {
                return;
            }

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

            // Only move camera back if we were actually focused
            if (_isFocused)
            {
                _isFocused = false;

                // Start smooth return
                if (_cameraCoroutine != null) StopCoroutine(_cameraCoroutine);
                _cameraCoroutine = StartCoroutine(MoveCameraToOriginal());
            }
            else
            {
                // UI update just in case
                UpdateUI(false);
            }
        }

        private GGJ_2026.Machines.MaskObject _hoveredMask;

        private void HandleRaycast()
        {
            Ray ray = new Ray(_cam.transform.position, _cam.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, _interactionDistance, _interactionLayer))
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                
                // --- Mask Hover Logic ---
                // Check if we hit a mask directly or parent
                var maskObj = hit.collider.GetComponent<GGJ_2026.Machines.MaskObject>();
                if (maskObj == null) maskObj = hit.collider.GetComponentInParent<GGJ_2026.Machines.MaskObject>();

                if (maskObj != null)
                {
                    if (_hoveredMask != maskObj)
                    {
                        // Clean up old
                        if (_hoveredMask != null) _hoveredMask.SetDescriptionVisibility(false);
                        
                        // Set new
                        _hoveredMask = maskObj;
                        _hoveredMask.SetDescriptionVisibility(true);
                    }
                }
                else
                {
                    // Hit something else (or nothing relevant), clear mask hover
                    if (_hoveredMask != null)
                    {
                        _hoveredMask.SetDescriptionVisibility(false);
                        _hoveredMask = null;
                    }
                }
                // ------------------------

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
                // Clear Mask Hover if hitting nothing
                if (_hoveredMask != null)
                {
                    _hoveredMask.SetDescriptionVisibility(false);
                    _hoveredMask = null;
                }
            }
        }

        private void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.E) && _currentInteractable != null)
            {
                StartInteraction();
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

        private void StartInteraction()
        {
            _isInteracting = true;
            _currentInteractable.OnInteract();
            
            // Hide Prompt
            UpdateUI(false);

            // Check if this interaction requires camera focus
            if (!_currentInteractable.UseCameraFocus)
            {
                _isFocused = false;
                // Do NOT lock movement or cursor
                // Do NOT move camera
                return;
            }

            _isFocused = true;

            // Disable Movement & Unlock Cursor
            if (_playerMovement != null)
            {
                _playerMovement.SetControl(false);
            }
            
            // Explicitly ensure cursor is free for interaction (Double check)
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Start Camera Move
            if (_currentInteractable.InteractionViewPoint != null)
            {
                // Store original local data (relative to player)
                _originalLocalPos = _cam.transform.localPosition;
                _originalLocalRot = _cam.transform.localRotation;

                if (_cameraCoroutine != null) StopCoroutine(_cameraCoroutine);
                _cameraCoroutine = StartCoroutine(MoveCameraToTarget(_currentInteractable.InteractionViewPoint));
            }
        }

        private IEnumerator MoveCameraToTarget(Transform target)
        {
            Vector3 startPos = _cam.transform.position;
            Quaternion startRot = _cam.transform.rotation;
            
            float time = 0f;

            while (time < _transitionDuration)
            {
                time += Time.deltaTime;
                float t = time / _transitionDuration;
                float curveValue = _transitionCurve.Evaluate(t);

                _cam.transform.position = Vector3.Lerp(startPos, target.position, curveValue);
                _cam.transform.rotation = Quaternion.Slerp(startRot, target.rotation, curveValue);

                yield return null;
            }

            // Ensure hard set at end
            _cam.transform.position = target.position;
            _cam.transform.rotation = target.rotation;
        }

        private IEnumerator MoveCameraToOriginal()
        {
            Vector3 startPos = _cam.transform.position;
            Quaternion startRot = _cam.transform.rotation;

            // Move back to cached local position relative to player
            Transform parent = _cam.transform.parent;
            Vector3 targetPos = parent.TransformPoint(_originalLocalPos);
            Quaternion targetRot = parent.rotation * _originalLocalRot;

            float time = 0f;

            while (time < _transitionDuration)
            {
                time += Time.deltaTime;
                float t = time / _transitionDuration;
                float curveValue = _transitionCurve.Evaluate(t);

                _cam.transform.position = Vector3.Lerp(startPos, targetPos, curveValue);
                _cam.transform.rotation = Quaternion.Slerp(startRot, targetRot, curveValue);

                yield return null;
            }

            // Hard reset to local to ensure precision for movement script
            _cam.transform.localPosition = _originalLocalPos;
            _cam.transform.localRotation = _originalLocalRot;

            // Re-enable controls AFTER camera is back
            if (_playerMovement != null)
            {
                _playerMovement.SetControl(true);
            }
        }
        
        public void LockAndMovePlayerTo(Transform playerTarget)
        {
            if (_cameraCoroutine != null)
                StopCoroutine(_cameraCoroutine);

            _isInteracting = true;
            _isFocused = true; // Assume this locks interactions too

            if (_playerMovement != null)
                _playerMovement.SetControl(false);

            _cameraCoroutine = StartCoroutine(MovePlayerTo(playerTarget));
        }

        private IEnumerator MovePlayerTo(Transform target)
        {
            Transform player = transform;

            Vector3 startPos = player.position;
            Quaternion startRot = player.rotation;

            float time = 0f;

            while (time < _transitionDuration)
            {
                time += Time.deltaTime;
                float t = time / _transitionDuration;
                float curveValue = _transitionCurve.Evaluate(t);

                player.position = Vector3.Lerp(
                    startPos,
                    target.position,
                    curveValue
                );

                player.rotation = Quaternion.Slerp(
                    startRot,
                    target.rotation,
                    curveValue
                );

                yield return null;
            }

            // Hard set
            player.position = target.position;
            player.rotation = target.rotation;
        }

        public void FreePlayer()
        {
            // Re-enable controls
            if (_playerMovement != null)
            {
                _playerMovement.SetControl(true);
            }
        }
    }
}
