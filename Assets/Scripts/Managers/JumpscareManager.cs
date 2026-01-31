using UnityEngine;
using System.Collections;

namespace GGJ_2026.Managers
{
    public class JumpscareManager : MonoBehaviour
    {
        [System.Serializable]
        public class JumpscareSpawnPoint
        {
            public Transform spawnTransform;
            public JumpscareType type;
            [Tooltip("Only for MovingDisappear type - target position to move to")]
            public Transform endPoint; // For MovingDisappear type
            public Sprite sprite;
        }

        public enum JumpscareType
        {
            MovingDisappear,    // Moves left and disappears
            StaticDisappear,    // Just disappears after being seen
            ApproachingDisappear // Approaches player then disappears
        }

        [Header("Jumpscare Settings")]
        [SerializeField] private GameObject _jumpscarePrefab; // Assign your scary object prefab
        [SerializeField] private JumpscareSpawnPoint[] _spawnPoints;

        [Header("Behavior Settings")]
        [SerializeField] private float _moveSpeed = 5f; // For MovingDisappear type
        [SerializeField] private float _disappearDelay = 0.5f; // For StaticDisappear type
        [SerializeField] private float _staticDelay = 0.5f; // For StaticDisappear type
        [SerializeField] private float _approachSpeed = 8f; // For ApproachingDisappear type
        [SerializeField] private float _approachStopDistance = 1.5f; // How close before disappearing

        [Header("Detection")]
        [SerializeField] private float _detectionDistance = 30f;
        [SerializeField] private float _detectionAngle = 45f; // Cone angle for detection
        [SerializeField] private LayerMask _jumpscareLayer; // Layer for jumpscare objects

        private GameObject _currentJumpscare;
        private bool _isJumpscareActive = false;
        private JumpscareType _currentType;
        private Transform _playerTransform;
        private Camera _playerCamera;
        private Coroutine _jumpscareCoroutine;
        private Transform _currentEndPoint; // Store endpoint for current jumpscare
        private SpriteRenderer _monsterSprRenderer;

        private void Awake()
        {
            _monsterSprRenderer = _jumpscarePrefab.GetComponentInChildren<SpriteRenderer>();
            // Find player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerTransform = player.transform;
                _playerCamera = player.GetComponentInChildren<Camera>();

                if (_playerCamera == null)
                {
                    _playerCamera = Camera.main;
                }
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                TriggerRandomJumpscare();
            }
        }

        public void TriggerRandomJumpscare()
        {
            if (_isJumpscareActive) return;
            if (_spawnPoints == null || _spawnPoints.Length == 0)
            {
                Debug.LogWarning("No spawn points assigned for Jumpscare!");
                return;
            }

            // Select random spawn point
            JumpscareSpawnPoint selectedPoint = _spawnPoints[Random.Range(0, _spawnPoints.Length)];

            // Spawn the jumpscare object
            SpawnJumpscare(selectedPoint);
        }

        private void SpawnJumpscare(JumpscareSpawnPoint spawnPoint)
        {
            _isJumpscareActive = true;
            _currentType = spawnPoint.type;
            _currentEndPoint = spawnPoint.endPoint; // Store endpoint reference

            // Instantiate the jumpscare object with ORIGINAL ROTATION
            _currentJumpscare = Instantiate(_jumpscarePrefab, spawnPoint.spawnTransform.position, spawnPoint.spawnTransform.rotation);

            // Set the layer for the jumpscare object and all its children
            SetLayerRecursively(_currentJumpscare, LayerMask.NameToLayer(GetLayerName(_jumpscareLayer)));

            // Play spawn sound
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayRandomSoundFromGroup("Jumpscare_Spawn");
            }

            // Start the appropriate coroutine based on type
            if (_jumpscareCoroutine != null) StopCoroutine(_jumpscareCoroutine);

            switch (_currentType)
            {
                case JumpscareType.MovingDisappear:
                    if (_currentEndPoint == null)
                    {
                        Debug.LogWarning("MovingDisappear type requires an EndPoint! Using default movement.");
                    }
                    _monsterSprRenderer.sprite = spawnPoint.sprite;
                    _jumpscareCoroutine = StartCoroutine(MovingDisappearBehavior());
                    Debug.LogWarning("JUMPSCARE TYPE 1: Moving Disappear");
                    break;
                case JumpscareType.StaticDisappear:
                    _monsterSprRenderer.sprite = spawnPoint.sprite;
                    _jumpscareCoroutine = StartCoroutine(StaticDisappearBehavior());
                    Debug.LogWarning("JUMPSCARE TYPE 2: Static Disappear");
                    break;
                case JumpscareType.ApproachingDisappear:
                    _monsterSprRenderer.sprite = spawnPoint.sprite;
                    _jumpscareCoroutine = StartCoroutine(ApproachingDisappearBehavior());
                    Debug.LogWarning("JUMPSCARE TYPE 3: Approaching Disappear");
                    break;
            }
        }

        private void SetLayerRecursively(GameObject obj, int layer)
        {
            if (obj == null) return;

            obj.layer = layer;

            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }

        private string GetLayerName(LayerMask mask)
        {
            // Get the first layer from the mask
            for (int i = 0; i < 32; i++)
            {
                if ((mask.value & (1 << i)) != 0)
                {
                    return LayerMask.LayerToName(i);
                }
            }
            return "Default";
        }

        private bool IsPlayerLookingAt()
        {
            if (_currentJumpscare == null || _playerCamera == null) return false;

            Vector3 directionToJumpscare = (_currentJumpscare.transform.position - _playerCamera.transform.position).normalized;
            float distance = Vector3.Distance(_playerCamera.transform.position, _currentJumpscare.transform.position);

            // Check if within detection distance
            if (distance > _detectionDistance)
            {
                return false;
            }

            // Check if within view cone
            float angle = Vector3.Angle(_playerCamera.transform.forward, directionToJumpscare);
            if (angle > _detectionAngle)
            {
                return false;
            }

            // Raycast from player camera to jumpscare with LAYER MASK
            Ray ray = new Ray(_playerCamera.transform.position, directionToJumpscare);
            if (Physics.Raycast(ray, out RaycastHit hit, _detectionDistance, _jumpscareLayer))
            {
                // Check if we hit the jumpscare object
                if (hit.collider.gameObject == _currentJumpscare ||
                    hit.collider.transform.IsChildOf(_currentJumpscare.transform))
                {
                    Debug.Log($"Player detected jumpscare: {hit.collider.name}");
                    return true;
                }
            }

            return false;
        }

        #region Jumpscare Behaviors

        private IEnumerator MovingDisappearBehavior()
        {
            // Wait until player looks
            while (!IsPlayerLookingAt())
                yield return null;

            if (SoundManager.Instance != null)
                SoundManager.Instance.PlayRandomSoundFromGroup("Jumpscare_Soft");

            // If endpoint is provided, move to it
            if (_currentEndPoint != null)
            {
                Vector3 startPos = _currentJumpscare.transform.position;
                Vector3 targetPos = _currentEndPoint.position;
                float distance = Vector3.Distance(startPos, targetPos);
                float duration = distance / _moveSpeed;

                float elapsed = 0f;

                // Move to endpoint while maintaining original rotation
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / duration;

                    _currentJumpscare.transform.position = Vector3.Lerp(startPos, targetPos, t);
                    // NO rotation change - keeps spawn rotation

                    // Check if reached endpoint
                    if (Vector3.Distance(_currentJumpscare.transform.position, targetPos) < 0.1f)
                    {
                        break;
                    }

                    yield return null;
                }
            }
            else
            {
                // Fallback: Move in the direction of spawn rotation's right (old behavior)
                float moveTime = 2f;
                float elapsed = 0f;

                Vector3 startPos = _currentJumpscare.transform.position;
                Vector3 moveDir = -_currentJumpscare.transform.right; // Use spawn rotation

                while (elapsed < moveTime)
                {
                    elapsed += Time.deltaTime;
                    _currentJumpscare.transform.position =
                        startPos + moveDir * _moveSpeed * elapsed;

                    yield return null;
                }
            }

            CleanupJumpscare();
        }

        private IEnumerator StaticDisappearBehavior()
        {
            while (!IsPlayerLookingAt())
                yield return null;

            if (SoundManager.Instance != null)
                SoundManager.Instance.PlayRandomSoundFromGroup("Jumpscare_Soft");

            yield return new WaitForSeconds(_staticDelay);

            CleanupJumpscare();
        }

        private IEnumerator ApproachingDisappearBehavior()
        {
            while (!IsPlayerLookingAt())
                yield return null;

            if (SoundManager.Instance != null)
                SoundManager.Instance.PlayRandomSoundFromGroup("Jumpscare_Loud");

            // Move towards player while KEEPING ORIGINAL ROTATION
            while (_currentJumpscare != null && _playerTransform != null)
            {
                float dist = Vector3.Distance(
                    _currentJumpscare.transform.position,
                    _playerTransform.position
                );

                if (dist <= _approachStopDistance)
                {
                    // Play final jumpscare sound
                    if (SoundManager.Instance != null)
                        SoundManager.Instance.PlayRandomSoundFromGroup("Jumpscare_Loud");

                    break;
                }

                Vector3 dir = (_playerTransform.position -
                               _currentJumpscare.transform.position).normalized;

                _currentJumpscare.transform.position +=
                    dir * _approachSpeed * Time.deltaTime;

                // NO ROTATION UPDATE - keeps spawn rotation!

                yield return null;
            }

            CleanupJumpscare();
        }

        #endregion

        private void CleanupJumpscare()
        {
            if (_currentJumpscare != null)
            {
                Destroy(_currentJumpscare);
            }

            _currentJumpscare = null;
            _isJumpscareActive = false;
            _jumpscareCoroutine = null;
            _currentEndPoint = null;
        }

        private void OnDestroy()
        {
            if (_jumpscareCoroutine != null)
            {
                StopCoroutine(_jumpscareCoroutine);
            }

            CleanupJumpscare();
        }

        // Debug visualization in editor
        private void OnDrawGizmosSelected()
        {
            if (_spawnPoints == null) return;

            foreach (var point in _spawnPoints)
            {
                if (point.spawnTransform != null)
                {
                    // Draw spawn point
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(point.spawnTransform.position, 0.5f);
                    Gizmos.DrawLine(point.spawnTransform.position,
                                   point.spawnTransform.position + point.spawnTransform.forward * 2f);

                    // Draw endpoint connection if exists (for MovingDisappear type)
                    if (point.type == JumpscareType.MovingDisappear && point.endPoint != null)
                    {
                        Gizmos.color = Color.green;
                        Gizmos.DrawWireSphere(point.endPoint.position, 0.3f);
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawLine(point.spawnTransform.position, point.endPoint.position);
                    }
                }
            }

            // Draw detection cone from camera if available
            if (_playerCamera != null && _isJumpscareActive && _currentJumpscare != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(_playerCamera.transform.position, _currentJumpscare.transform.position);
            }
        }
    }
}