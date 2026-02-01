using UnityEngine;
using GGJ_2026.Interactions;
using GGJ_2026.Managers;
using GGJ_2026.Data;

namespace GGJ_2026.Machines
{
    public class MaskObject : MonoBehaviour, IInteractable
    {
        [Header("Data")]
        [SerializeField] private MaskData _maskData;
        
        // Visuals
        [SerializeField] private Transform _viewPoint;
        [SerializeField] private SpriteRenderer _iconRenderer;

        [SerializeField] private TMPro.TMP_Text _descriptionText;

        [SerializeField] AudioClip selectMask; //

        private AudioSource _audioSource;
        public string InteractionPrompt => $"Press E to choose: {_maskData?.MaskName}";
        public Transform InteractionViewPoint => _viewPoint;
        public bool UseCameraFocus => false; // Masks don't lock camera

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        private void Update()
        {
            // Billboard Effect: Always face the camera
            if (_descriptionText != null && _descriptionText.gameObject.activeSelf)
            {
                // Simple billboard: face the camera position
                // Rotate 180 if text is backward, or just use LookAt(camera)
                // Often text needs to look AT camera but might be flipped. 
                // Creating a standard LookAt.
                _descriptionText.transform.LookAt(Camera.main.transform);
                _descriptionText.transform.Rotate(0, 180, 0); // Corrects if text is mirrored
            }
        }

        public void Initialize(MaskData data)
        {
            _maskData = data;
            if (_maskData != null)
            {
                if (_iconRenderer != null)
                    _iconRenderer.sprite = _maskData.Icon;
                
                if (_descriptionText != null)
                {
                    _descriptionText.text = _maskData.Description;
                    _descriptionText.gameObject.SetActive(false); // Hide by default
                }
            }
        }

        public void SetDescriptionVisibility(bool visible)
        {
            if (_descriptionText != null)
            {
                _descriptionText.gameObject.SetActive(visible);
            }
        }

        public void OnInteract()
        {
            if (_maskData == null) return;

            Debug.Log($"Picked Mask: {_maskData.MaskName}");


            // Confirm selection via Manager
            if (MaskManager.Instance != null)
            {
                MaskManager.Instance.SelectMaskFromObject(_maskData);
            }
        }

        public void OnExit()
        {
            // Optional: Close description UI if implemented
        }

        public void OnInteractStay()
        {
            //throw new System.NotImplementedException();
        }
    }
}
