using UnityEngine;
using TMPro;
using GGJ_2026.Managers;

namespace GGJ_2026.UI
{
    public class DashboardUI : MonoBehaviour
    {
        [Header("UI Fields")]
        [SerializeField] private TextMeshProUGUI _electricityText;
        [SerializeField] private TextMeshProUGUI _sanityText;
        [SerializeField] private TextMeshProUGUI _distanceText;
        [SerializeField] private TextMeshProUGUI _frequencyText;

        // Optional Bars (Sliders or Images)
        // [SerializeField] private Image _electricityBar; 

        private void Start()
        {
            SubscribeToEvents();
            // Initial update
            if (ResourceManager.Instance != null)
            {
                UpdateElectricity(ResourceManager.Instance.GetElectricity());
                UpdateSanity(ResourceManager.Instance.GetSanity());
                UpdateDistance(ResourceManager.Instance.GetDistance());
                UpdateFrequency(ResourceManager.Instance.GetFrequency());
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnElectricityChanged += UpdateElectricity;
                ResourceManager.Instance.OnSanityChanged += UpdateSanity;
                ResourceManager.Instance.OnDistanceChanged += UpdateDistance;
                ResourceManager.Instance.OnFrequencyChanged += UpdateFrequency;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnElectricityChanged -= UpdateElectricity;
                ResourceManager.Instance.OnSanityChanged -= UpdateSanity;
                ResourceManager.Instance.OnDistanceChanged -= UpdateDistance;
                ResourceManager.Instance.OnFrequencyChanged -= UpdateFrequency;
            }
        }

        private void UpdateElectricity(float value)
        {
            if (_electricityText != null) 
                _electricityText.text = $"Electricity: {value:F0}%";
        }

        private void UpdateSanity(float value)
        {
            if (_sanityText != null)
                _sanityText.text = $"Sanity: {value:F0}%";
        }

        private void UpdateDistance(float value)
        {
            if (_distanceText != null)
                _distanceText.text = $"Distance: {value:F0}m";
        }

        private void UpdateFrequency(float value)
        {
            if (_frequencyText != null)
                _frequencyText.text = $"Frequency: {value:F0}%";
        }
    }
}
