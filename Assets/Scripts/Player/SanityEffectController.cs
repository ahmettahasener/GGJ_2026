using UnityEngine;
using UnityEngine.Rendering; // Core Volume Framework
// using UnityEngine.Rendering.Universal; // Common for URP, but might break if Built-in. 
// We will try to rely on generic references or reflection if needed, 
// BUT for a Jam, direct URP reference is standard probability. 
// However, to be SAFE and ROBUST without knowing pipeline:
// I will use a serialized VOLUME reference, and try to get components dynamically.
// Note: Vignette and ChromaticAberration classes often require specific namespaces.
// I will assume UnityEngine.Rendering.Universal for now as it's the safest 'modern' beat.
// If compilation fails, user will notify.

namespace GGJ_2026.Player
{
    public class SanityEffectController : MonoBehaviour
    {
        [Header("Post Processing")]
        [SerializeField] private Volume _volume; // Check Unity Registry for 'Volume'
        
        // We need specific types for the overrides. 
        // Since we can't be 100% sure of namespace, we'll try to find them or fallback to simple logs if missing.
        // Actually, let's look for the standard Universal ones.
        
        private UnityEngine.Rendering.Universal.Vignette _vignette;
        private UnityEngine.Rendering.Universal.ChromaticAberration _chromaticAberration;
        
        // Fallback for Built-in PostProcessing Stack v2 (just in case user has that)
        // private UnityEngine.Rendering.PostProcessing.PostProcessVolume _ppVolume;

        private void Start()
        {
            if (_volume != null && _volume.profile != null)
            {
                _volume.profile.TryGet(out _vignette);
                _volume.profile.TryGet(out _chromaticAberration);
            }
            else
            {
                Debug.LogWarning("SanityEffectController: No Volume Assigned!");
            }

            // Subscribe
            if (Managers.ResourceManager.Instance != null)
            {
                Managers.ResourceManager.Instance.OnSanityChanged += HandleSanityChanged;
                // Init
                HandleSanityChanged(Managers.ResourceManager.Instance.GetSanity());
            }
        }

        private void OnDestroy()
        {
            if (Managers.ResourceManager.Instance != null)
            {
                Managers.ResourceManager.Instance.OnSanityChanged -= HandleSanityChanged;
            }
        }

        private void HandleSanityChanged(float sanity)
        {
            // Effect starts at 50% sanity
            float threshold = 50f;
            
            if (sanity >= threshold)
            {
                SetEffects(0f);
                return;
            }

            // Map 50->0 to 0->1 intensity
            // 50 sanity = 0 effect
            // 0 sanity = 1 effect
            float t = 1f - (sanity / threshold);

            SetEffects(t);
        }

        private void SetEffects(float intensity)
        {
            // Vignette usually goes 0 to 0.5 or 1
            if (_vignette != null)
            {
                _vignette.intensity.value = Mathf.Lerp(0f, 0.5f, intensity);
                _vignette.active = intensity > 0;
            }

            // Chromatic Aberration usually 0 to 1
            if (_chromaticAberration != null)
            {
                _chromaticAberration.intensity.value = Mathf.Lerp(0f, 1f, intensity);
                _chromaticAberration.active = intensity > 0;
            }
        }
    }
}
