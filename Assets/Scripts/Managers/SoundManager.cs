using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace GGJ_2026.Managers
{
    [System.Serializable]
    public struct SoundGroup
    {
        public string GroupName;
        public List<AudioClip> Clips;
    }

    [RequireComponent(typeof(AudioSource))]
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [Header("Audio Source")]
        [SerializeField] private AudioSource _sfxSource;

        [Header("Sound Libraries")]
        [SerializeField] private List<SoundGroup> _soundGroups = new List<SoundGroup>();

        private Dictionary<string, List<AudioClip>> _soundDictionary;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeDictionary();
            }
            else
            {
                Destroy(gameObject);
            }

            if (_sfxSource == null)
            {
                _sfxSource = GetComponent<AudioSource>();
            }
        }

        private void InitializeDictionary()
        {
            _soundDictionary = new Dictionary<string, List<AudioClip>>();
            foreach (var group in _soundGroups)
            {
                if (!_soundDictionary.ContainsKey(group.GroupName))
                {
                    _soundDictionary.Add(group.GroupName, group.Clips);
                }
            }
        }

        /// <summary>
        /// Plays a specific AudioClip once.
        /// </summary>
        public void PlaySound(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;
            _sfxSource.PlayOneShot(clip, volume);
        }

        /// <summary>
        /// Plays a random sound from the specified group name.
        /// Useful for paranormal events or repetitive machine noises.
        /// </summary>
        public void PlayRandomSoundFromGroup(string groupName, float volume = 1f)
        {
            if (_soundDictionary.TryGetValue(groupName, out List<AudioClip> clips) && clips.Count > 0)
            {
                var randomClip = clips[UnityEngine.Random.Range(0, clips.Count)];
                PlaySound(randomClip, volume);
            }
            else
            {
                Debug.LogWarning($"SoundGroup '{groupName}' not found or empty!");
            }
        }
    }
}
