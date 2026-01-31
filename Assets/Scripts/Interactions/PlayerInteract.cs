using UnityEngine;
using GGJ_2026.UI;
using System.Collections;

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

        // Camera State
        private Vector3 _originalLocalPos;
        private Quaternion _originalLocalRot;
        private Coroutine _cameraCoroutine;

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

        public void ForceExitInteraction()
        {
            ExitInteraction();
        }

        private void HandleExitInput()
        {
            // Block ESC if we are in EndNight state (handled by GameManager logic or check here)
            // But getting GameManager dependency here might be circular or messy. 
            // We'll rely on GameManager preventing input or this script dealing with it.
            // But user asked to "disable ESC".
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

            // Start smooth return
            if (_cameraCoroutine != null) StopCoroutine(_cameraCoroutine);
            _cameraCoroutine = StartCoroutine(MoveCameraToOriginal());

            // UI update
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

            // Disable Movement
            if (_playerMovement != null)
            {
                _playerMovement.SetControl(false);
            }

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
                // If we exit mid-transition, loop breaks because _isInteracting goes false? 
                // No, ExitInteraction starts a new coroutine which stops this one.
                
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

            // We want to return to local coordinates relative to the player parent logic, 
            // but simply putting it back to saved locals relative to parent is enough 
            // because parent hasn't moved (control locked).
            
            // However, Lerp needs World Space end goals if we are moving in world space.
            // _cam.transform.parent should be the Player object.
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
    }
}
