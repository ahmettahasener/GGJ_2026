using UnityEngine;
using System.Collections;

namespace GGJ_2026.Managers
{
    public class ParanormalManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _minEventInterval = 10f;
        [SerializeField] private float _maxEventInterval = 60f;
        [SerializeField] private float _sanityThreshold = 70f; // Starts happening below this

        [Header("Event Types")]
        [SerializeField] private Light[] _roomLights; // Assign in inspector
        
        private float _timer;
        private float _currentInterval;

        private void Start()
        {
            SetNextInterval();
        }

        private void Update()
        {
            if (ResourceManager.Instance == null) return;
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Gameplay) return;

            float currentSanity = ResourceManager.Instance.GetSanity();

            // Only run if sanity is low enough
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
            
            // Interval gets shorter as sanity drops
            // Lerps between max and min based on Sanity (0 to 70)
            float sanity = ResourceManager.Instance != null ? ResourceManager.Instance.GetSanity() : 100f;
            float t = 1f - Mathf.Clamp01(sanity / 100f); 
            // t=0 (High Sanity) -> Max Interval
            // t=1 (Low Sanity) -> Min Interval
            
            _currentInterval = Mathf.Lerp(_maxEventInterval, _minEventInterval, t);
        }

        private void TriggerEvent(float sanity)
        {
            // Pick a random event type
            int roll = Random.Range(0, 3);
            
            // Higher chance for intense events if sanity is very low
            if (sanity < 30f) roll = Random.Range(0, 4); 

            Debug.Log($"Paranormal Event Triggered! (Roll: {roll})");

            switch (roll)
            {
                case 0:
                    // Sound: Whispers or Creaks
                    SoundManager.Instance.PlayRandomSoundFromGroup("Ambience_Creepy");
                    break;
                case 1:
                    // Sound: Sudden Bang or Knock
                    SoundManager.Instance.PlayRandomSoundFromGroup("Jumpscare_Soft");
                    break;
                case 2:
                    // Visual: Lights Flicker
                    StartCoroutine(FlickerLights());
                    break;
                case 3:
                     // Intense: Loud noise + Light kill
                     SoundManager.Instance.PlayRandomSoundFromGroup("Jumpscare_Loud");
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
                float originalIntensity = light.intensity;
                for (int i = 0; i < flickers; i++)
                {
                    light.enabled = !light.enabled;
                    // Randomize intensity slightly?
                    yield return new WaitForSeconds(Random.Range(0.05f, duration));
                }
                light.enabled = true;
                light.intensity = originalIntensity;
            }
        }
    }
}
