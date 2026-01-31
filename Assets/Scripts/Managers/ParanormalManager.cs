using UnityEngine;
using System.Collections;

namespace GGJ_2026.Managers
{
    public class ParanormalManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _minEventInterval = 10f;
        [SerializeField] private float _maxEventInterval = 60f;
        [SerializeField] private float _sanityThreshold = 70f;

        [Header("Event Types")]
        [SerializeField] private Light[] _roomLights;

        [Header("Jumpscare System")]
        [SerializeField] private JumpscareManager _jumpscareManager;

        [Header("Audio")]
        [SerializeField] private AudioSource _audioSource; // Paranormal olaylar için tek AudioSource

        private float _timer;
        private float _currentInterval;

        private void Awake()
        {
            // Auto-find AudioSource if not assigned
            if (_audioSource == null)
            {
                _audioSource = GetComponent<AudioSource>();
                if (_audioSource == null)
                {
                    _audioSource = gameObject.AddComponent<AudioSource>();
                }
            }
        }

        private void Start()
        {
            SetNextInterval();
        }

        private void Update()
        {
            if (ResourceManager.Instance == null) return;
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Gameplay) return;

            float currentSanity = ResourceManager.Instance.GetSanity();

            if (currentSanity > _sanityThreshold) return;

            _timer += Time.deltaTime;

            if (_timer >= _currentInterval)
            {
                TriggerEvent(currentSanity);
                SetNextInterval();
            }
        }

        private void SetNextInterval()
        {
            _timer = 0f;
            float sanity = ResourceManager.Instance != null ? ResourceManager.Instance.GetSanity() : 100f;
            float t = 1f - Mathf.Clamp01(sanity / 100f);
            _currentInterval = Mathf.Lerp(_maxEventInterval, _minEventInterval, t);
        }

        private void TriggerEvent(float sanity)
        {
            int roll = Random.Range(0, 4);

            if (sanity < 30f)
            {
                roll = Random.Range(0, 5);
            }

            Debug.Log($"Paranormal Event Triggered! (Roll: {roll}, Sanity: {sanity:F1})");

            switch (roll)
            {
                case 0:
                    // Whispers - Kendi AudioSource'unu kullan
                    if (SoundManager.Instance != null)
                    {
                        SoundManager.Instance.PlayRandomSoundFromGroup(_audioSource, "Ambience_Creepy");
                    }
                    break;

                case 1:
                    // Bang/Knock - Kendi AudioSource'unu kullan
                    if (SoundManager.Instance != null)
                    {
                        SoundManager.Instance.PlayRandomSoundFromGroup(_audioSource, "Jumpscare_Soft");
                    }
                    break;

                case 2:
                    // Lights Flicker
                    StartCoroutine(FlickerLights());
                    break;

                case 3:
                    // Jumpscare
                    if (_jumpscareManager != null)
                    {
                        _jumpscareManager.TriggerRandomJumpscare();
                    }
                    break;

                case 4:
                    // Intense - Kendi AudioSource'unu kullan
                    if (SoundManager.Instance != null)
                    {
                        SoundManager.Instance.PlayRandomSoundFromGroup(_audioSource, "Jumpscare_Loud");
                    }
                    StartCoroutine(FlickerLights(true));
                    break;
            }
        }

        private IEnumerator FlickerLights(bool heavy = false)
        {
            if (_roomLights == null || _roomLights.Length == 0) yield break;

            int flickers = heavy ? 10 : 3;
            float duration = heavy ? 0.05f : 0.1f;

            foreach (var light in _roomLights)
            {
                if (light == null) continue;

                float originalIntensity = light.intensity;

                for (int i = 0; i < flickers; i++)
                {
                    light.enabled = !light.enabled;
                    if (light.enabled)
                    {
                        light.intensity = originalIntensity * Random.Range(0.5f, 1.5f);
                    }
                    yield return new WaitForSeconds(Random.Range(0.05f, duration));
                }

                light.enabled = true;
                light.intensity = originalIntensity;
            }
        }
    }
}