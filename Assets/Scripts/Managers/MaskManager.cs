using UnityEngine;
using System.Collections.Generic;
using GGJ_2026.Data;
using System.Linq;

namespace GGJ_2026.Managers
{
    public class MaskManager : MonoBehaviour
    {
        public static MaskManager Instance { get; private set; }

        [Header("Mask Library")]
        [SerializeField] private List<MaskData> _allMasks;

        public MaskData ActiveMask { get; private set; }

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

        public List<MaskData> GetRandomMasks(int count = 3)
        {
            if (_allMasks == null || _allMasks.Count == 0) return new List<MaskData>();
            
            // Simple shuffle and take
            return _allMasks.OrderBy(x => UnityEngine.Random.value).Take(Mathf.Min(count, _allMasks.Count)).ToList();
        }

        public void ActivateMask(MaskData mask)
        {
            ActiveMask = mask;
            Debug.Log($"Mask Activated: {mask.MaskName} - {mask.Description}");
        }

        public void ClearMask()
        {
            ActiveMask = null;
        }

        public bool IsEffectActive(MaskType type)
        {
            return ActiveMask != null && ActiveMask.Type == type;
        }
    }
}
