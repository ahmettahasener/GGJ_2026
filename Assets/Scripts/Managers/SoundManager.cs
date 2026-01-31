using UnityEngine;
using System.Collections.Generic;

namespace GGJ_2026.Managers
{
    [System.Serializable]
    public struct SoundGroup
    {
        public string GroupName;
        public List<AudioClip> Clips;
    }

    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

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
        /// Plays a specific AudioClip on a provided AudioSource.
        /// </summary>
        public void PlaySound(AudioSource source, AudioClip clip, float volume = 1f)
        {
            if (source == null || clip == null) return;
            source.PlayOneShot(clip, volume);
        }

        /// <summary>
        /// Plays a random sound from the specified group on a provided AudioSource.
        /// </summary>
        public void PlayRandomSoundFromGroup(AudioSource source, string groupName, float volume = 1f)
        {
            if (source == null)
            {
                Debug.LogWarning("AudioSource is null!");
                return;
            }

            if (_soundDictionary.TryGetValue(groupName, out List<AudioClip> clips) && clips.Count > 0)
            {
                var randomClip = clips[Random.Range(0, clips.Count)];
                PlaySound(source, randomClip, volume);
            }
            else
            {
                Debug.LogWarning($"SoundGroup '{groupName}' not found or empty!");
            }
        }

        /// <summary>
        /// Gets a random clip from a sound group without playing it.
        /// Useful for getting clips to play on specific AudioSources.
        /// </summary>
        public AudioClip GetRandomClipFromGroup(string groupName)
        {
            if (_soundDictionary.TryGetValue(groupName, out List<AudioClip> clips) && clips.Count > 0)
            {
                return clips[Random.Range(0, clips.Count)];
            }

            Debug.LogWarning($"SoundGroup '{groupName}' not found or empty!");
            return null;
        }

        /// <summary>
        /// Gets a specific clip from a group by index.
        /// </summary>
        public AudioClip GetClipFromGroup(string groupName, int index)
        {
            if (_soundDictionary.TryGetValue(groupName, out List<AudioClip> clips) &&
                index >= 0 && index < clips.Count)
            {
                return clips[index];
            }

            Debug.LogWarning($"SoundGroup '{groupName}' clip at index {index} not found!");
            return null;
        }
    }
}