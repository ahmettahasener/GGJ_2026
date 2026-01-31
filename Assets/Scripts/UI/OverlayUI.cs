using UnityEngine;
using TMPro; // Assuming TextMeshPro is used, if not standard Text, but TMP is standard now.
// If user has not specified TMP, I will use UnityEngine.UI for safety or TextMeshPro if I check packages? 
// Defaulting to TMP is risky if not installed, but standard in 2026 context. 
// However, to be safe with "vanilla" requests without package checks, I'll use a generic approach or standard UI, 
// but TMP is best practice. Let's use UnityEngine.UI.Text for simplicity or check. 
// Actually, I'll use TMPro as it's the Unity default for text now.
using TMPro;
using UnityEngine.UI;

namespace GGJ_2026.UI
{
    public class OverlayUI : MonoBehaviour
    {
        public static OverlayUI Instance { get; private set; }

        [Header("Transition Elements")]
        [SerializeField] private CanvasGroup _fadeGroup;
        [SerializeField] private Image _fadeScreen;
        [SerializeField] private TextMeshProUGUI _nightText;
        [SerializeField] private TextMeshProUGUI _interactionText;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            HidePrompt();
            
            // Ensure screen is clear at start, or maybe black if we want intro.
            // Let's assume clear.
            if (_fadeGroup != null)
            {
                _fadeGroup.alpha = 0f;
                _fadeGroup.blocksRaycasts = false;
            }
            if (_nightText != null) _nightText.gameObject.SetActive(false);
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

        // --- Transition Methods ---

        public void SetNightText(string text)
        {
            if (_nightText != null)
            {
                _nightText.text = text;
                _nightText.gameObject.SetActive(true);
            }
        }

        public void HideNightText()
        {
            if (_nightText != null) _nightText.gameObject.SetActive(false);
        }

        public System.Collections.IEnumerator FadeIn(float duration)
        {
            if (_fadeGroup == null)
            {
                Debug.LogWarning("OverlayUI: FadeGroup is missing!");
                yield break;
            }

            _fadeGroup.blocksRaycasts = true; // Block input
            float startAlpha = _fadeGroup.alpha;
            float time = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;
                _fadeGroup.alpha = Mathf.Lerp(startAlpha, 1f, time / duration);
                _fadeScreen.color = new Color(0,0,0,Mathf.Lerp(startAlpha, 1f, time / duration));
                yield return null;
            }
            _fadeGroup.alpha = 1f;
        }

        public System.Collections.IEnumerator FadeOut(float duration)
        {
            if (_fadeGroup == null)
            {
                Debug.LogWarning("OverlayUI: FadeGroup is missing!");
                yield break;
            }

            float startAlpha = _fadeGroup.alpha;
            float time = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;
                _fadeGroup.alpha = Mathf.Lerp(startAlpha, 0f, time / duration);
                yield return null;
            }
            _fadeGroup.alpha = 0f;
            _fadeGroup.blocksRaycasts = false; // Allow input
        }
    }
}
