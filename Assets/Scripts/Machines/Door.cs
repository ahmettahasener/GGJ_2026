using UnityEngine;
using GGJ_2026.Managers;
using System.Collections;
using GGJ_2026.Interactions;

namespace GGJ_2026.Machines
{
    public class Door : MachineBase, IInteractable
    {
        [Header("Door Settings")]
        [SerializeField] private float _openAngle = 90f; // Açýlma açýsý
        [SerializeField] private float _openDuration = 1.0f; // Açýlma süresi
        [SerializeField] private bool _isOpen = false; // Baþlangýç durumu
        [SerializeField] private bool canOpenable = true;

        [SerializeField] AudioClip door;
        public bool GetCanOpen
        {
            get
            {
                return canOpenable;
            }
            set
            {
                canOpenable = value;
            }
        }

        [Header("Pivot Settings")]
        [SerializeField] private Transform _doorPivot; // Kapýnýn pivot noktasý (boþ býrakýlýrsa kendi transform'u kullanýlýr)

        private bool _isAnimating = false;
        private Quaternion _closedRotation;
        private Quaternion _openRotation;

        private FuseBox _fuseBox;

        private void Awake()
        {
            _fuseBox = FindAnyObjectByType<FuseBox>();

            _electricityCost = 0f; // Kapý elektrik tüketmez

            // Pivot ayarý
            if (_doorPivot == null)
            {
                _doorPivot = transform;
            }

            // Baþlangýç rotasyonlarýný kaydet
            _closedRotation = _doorPivot.localRotation;
            _openRotation = _closedRotation * Quaternion.Euler(0f, _openAngle, 0f);
        }
        private void OnEnable()
        {
            GameManager.Instance.gameOverEvent += ForceOpen;
            GameManager.Instance.gameWinEvent += ForceOpen;
            GameManager.Instance.newNightEvent += ForceClose;
            _fuseBox.fuseExpEvent += ForceOpen;
        }
        private void OnDisable()
        {
            GameManager.Instance.gameOverEvent -= ForceOpen;
            GameManager.Instance.gameWinEvent -= ForceOpen;
            GameManager.Instance.newNightEvent -= ForceClose;
            _fuseBox.fuseExpEvent -= ForceOpen;
        }

        private void Start()
        {
            // Baþlangýç durumuna göre pozisyon ayarla
            if (_isOpen)
            {
                _doorPivot.localRotation = _openRotation;
            }
        }

        public override void OnInteract()
        {
            if (_isAnimating || !canOpenable) return; // Animasyon sýrasýnda etkileþim engelle

            if (_isOpen)
            {
                StartCoroutine(CloseDoor());
            }
            else
            {
                StartCoroutine(OpenDoor());
            }
        }

        private IEnumerator OpenDoor()
        {
            _audioSource.PlayOneShot(door);

            _isAnimating = true;
            Debug.Log("Kapý açýlýyor...");

            float elapsed = 0f;
            Quaternion startRotation = _doorPivot.localRotation;

            while (elapsed < _openDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _openDuration;

                // Smooth interpolation
                _doorPivot.localRotation = Quaternion.Slerp(startRotation, _openRotation, t);

                yield return null;
            }

            _doorPivot.localRotation = _openRotation;
            _isOpen = true;
            _isAnimating = false;
            Debug.Log("Kapý açýldý!");
        }

        private IEnumerator CloseDoor()
        {
            _audioSource.PlayOneShot(door);

            _isAnimating = true;
            Debug.Log("Kapý kapanýyor...");

            float elapsed = 0f;
            Quaternion startRotation = _doorPivot.localRotation;

            while (elapsed < _openDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _openDuration;

                // Smooth interpolation
                _doorPivot.localRotation = Quaternion.Slerp(startRotation, _closedRotation, t);

                yield return null;
            }

            _doorPivot.localRotation = _closedRotation;
            _isOpen = false;
            _isAnimating = false;
            Debug.Log("Kapý kapandý!");
        }
        /// <summary>
        /// Kapýyý otomatik olarak açar (zaten açýksa çalýþmaz)
        /// </summary>
        public void Open()
        {
            if (_isAnimating || _isOpen || !canOpenable) return;

            StartCoroutine(OpenDoor());
        }
        /// <summary>
        /// Kapýyý otomatik olarak kapatýr (zaten kapalýysa çalýþmaz)
        /// </summary>
        public void Close()
        {
            if (_isAnimating || !_isOpen || !canOpenable) return;
            StartCoroutine(CloseDoor());
        }
        /// <summary>
        /// Kapýnýn durumunu otomatik olarak deðiþtirir
        /// </summary>
        public void ToggleDoor()
        {
            if (_isAnimating || !canOpenable) return;

            if (_isOpen)
                Close();
            else
                Open();
        }

        public void ForceOpen()
        {
            Debug.LogWarning("ForceOpen");
            StartCoroutine(OpenDoor());
        }
        public void ForceClose()
        {
            StartCoroutine(CloseDoor());
        }
    }
}