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

        public string InteractionPrompt => $"Press E to choose: {_maskData?.MaskName}";
        public Transform InteractionViewPoint => _viewPoint;

        public void Initialize(MaskData data)
        {
            _maskData = data;
            if (_maskData != null && _iconRenderer != null)
            {
                _iconRenderer.sprite = _maskData.Icon;
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
    }
}
