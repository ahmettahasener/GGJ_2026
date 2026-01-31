using UnityEngine;
using TMPro; // Assuming TextMeshPro is used, if not standard Text, but TMP is standard now.
// If user has not specified TMP, I will use UnityEngine.UI for safety or TextMeshPro if I check packages? 
// Defaulting to TMP is risky if not installed, but standard in 2026 context. 
// However, to be safe with "vanilla" requests without package checks, I'll use a generic approach or standard UI, 
// but TMP is best practice. Let's use UnityEngine.UI.Text for simplicity or check. 
// Actually, I'll use TMPro as it's the Unity default for text now.
using TMPro; 

namespace GGJ_2026.UI
{
    public class OverlayUI : MonoBehaviour
    {
        public static OverlayUI Instance { get; private set; }

        [Header("UI Elements")]
        [SerializeField] private GameObject _reticle;
        [SerializeField] private TextMeshProUGUI _interactionText; 

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            HidePrompt();
        }

        public void ShowPrompt(string message)
        {
            if (_interactionText != null)
            {
                _interactionText.text = message;
                _interactionText.gameObject.SetActive(true);
            }
        }

        public void HidePrompt()
        {
            if (_interactionText != null)
            {
                _interactionText.gameObject.SetActive(false);
            }
        }
    }
}
