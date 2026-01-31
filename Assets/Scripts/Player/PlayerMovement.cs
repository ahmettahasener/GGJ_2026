using UnityEngine;

namespace GGJ_2026.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _gravity = -9.81f;
        [SerializeField] private float _jumpHeight = 1.0f;

        [Header("Look")]
        [SerializeField] private float _mouseSensitivity = 100f;
        [SerializeField] private Transform _cameraTransform;

        private CharacterController _controller;
        private Vector3 _velocity;
        private bool _isGrounded;
        private float _xRotation = 0f;
        
        // State
        private bool _canMove = true;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            
            // Auto-find camera if not assigned
            if (_cameraTransform == null)
            {
                _cameraTransform = GetComponentInChildren<Camera>()?.transform;
            }

            LockCursor();
        }

        private void Update()
        {
            if (!_canMove) return;

            HandleLook();
            HandleMovement();
        }

        private void HandleLook()
        {
            if (_cameraTransform == null) return;

            float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity * Time.deltaTime;

            _xRotation -= mouseY;
            _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

            _cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
            transform.Rotate(Vector3.up * mouseX);
        }

        private void HandleMovement()
        {
            _isGrounded = _controller.isGrounded; // Simple ground check
            if (_isGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f;
            }

            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            Vector3 move = transform.right * x + transform.forward * z;
            _controller.Move(move * _moveSpeed * Time.deltaTime);

            //// Jump
            //if (Input.GetButtonDown("Jump") && _isGrounded)
            //{
            //    _velocity.y = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
            //}

            // Gravity
            //_velocity.y += _gravity * Time.deltaTime;
            _controller.Move(_velocity * Time.deltaTime);
        }

        public void SetControl(bool active)
        {
            _canMove = active;
            if (active)
            {
                LockCursor();
                // Optional: Reset camera rotation or Keep previous? Usually keep.
            }
            else
            {
                UnlockCursor();
            }
        }

        private void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
