using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

namespace GGJ_2026.UI
{
    public class InfoBox : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Animation Settings")]
        [SerializeField] private float _displayDuration = 3.0f;
        [SerializeField] private float _fadeDuration = 0.5f;

        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
        }

        public void Setup(string message, Sprite icon, Color textColor)
        {
            if (_messageText != null)
            {
                _messageText.text = message;
                _messageText.color = textColor;
            }

            if (_iconImage != null)
            {
                _iconImage.sprite = icon;
                _iconImage.gameObject.SetActive(icon != null);
            }

            StartCoroutine(AnimateRoutine());
        }

        private IEnumerator AnimateRoutine()
        {
            // 1. Init (Invisible)
            _canvasGroup.alpha = 0f;

            // 2. Fade In
            float time = 0f;
            while (time < _fadeDuration)
            {
                time += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Lerp(0f, 1f, time / _fadeDuration);
                yield return null;
            }
            _canvasGroup.alpha = 1f;

            // 3. Stay
            yield return new WaitForSeconds(_displayDuration);

            // 4. Fade Out
            time = 0f;
            while (time < _fadeDuration)
            {
                time += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Lerp(1f, 0f, time / _fadeDuration);
                yield return null;
            }
            _canvasGroup.alpha = 0f;

            // 5. Destroy
            Destroy(gameObject);
        }
    }
}
